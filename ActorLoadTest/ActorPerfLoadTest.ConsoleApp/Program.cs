using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActorCreator.Interfaces;
using ActorTest.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Client;
using Newtonsoft.Json;
using Resiliency;
using Resiliency.CircuitBreaker;
using Resiliency.Command;
using Resiliency.Metrics;

namespace ActorPerfLoadTest.ConsoleApp {
  class Program {
    static void Main(string[] args) {
      RunAsync().GetAwaiter().GetResult();
    }

    const string ServiceUri = "fabric:/ActorLoadTest/ActorTestActorService";
    const string CreateActorServiceUri = "fabric:/ActorLoadTest/ActorCreator";
    private static int created;
    private static int timedout;
    private static int exception;

    static async Task RunAsync() {
      var noOfMsgsPerIteration = GetIterations();
      var seconds = GetRestPeriodMilliseconds();
      var sequentialOrConcurrent = GetSequentialOrConcurrentProcessing();

      var restTime = TimeSpan.FromMilliseconds(seconds);

      int messageCount = 0;

      Task.Factory.StartNew(MetricsTimer, TaskCreationOptions.LongRunning);

      while (true) {
        var id = Guid.NewGuid().ToString();
        
        var proxy = CreateProxy<IActorTest>(new Uri(ServiceUri), id);
        var createActorProxy = CreateProxy(new Uri(CreateActorServiceUri), id);

        Console.WriteLine("Sending");

        if (sequentialOrConcurrent == 1) {
          try {
            ProcessCreateActorTasksSequentially(noOfMsgsPerIteration, createActorProxy, id);
            Interlocked.Increment(ref created);
          }
          catch (OperationCanceledException) {
            Interlocked.Increment(ref timedout);
          }
          catch (Exception) {
            Interlocked.Increment(ref exception);
          }
        }
        else {
          await ProcessTasksConcurrently(noOfMsgsPerIteration, proxy, id);

        }

        messageCount += noOfMsgsPerIteration;

        Console.WriteLine($"Sent {noOfMsgsPerIteration} messages. Total {messageCount}.");

        await Task.Delay(restTime);
      }
    }

    private static async Task MetricsTimer() {
      while (true) {
        DumpMetricsToConsole();
        await Task.Delay(1000);
      }
    }

    private static void ProcessTasksSequentially(int noOfMsgsPerIteration, IActorTest proxy, string id) {
      for (int i = 0; i < noOfMsgsPerIteration; i++) {
        CreateAndRunTask(proxy, id).Wait();
      }
    }

    private static void ProcessCreateActorTasksSequentially(int noOfMsgsPerIteration, IActorCreator proxy, string id) {
      for (int i = 0; i < noOfMsgsPerIteration; i++) {
        CreateAndRunCreateActorTask(proxy, id).Wait();
      }
    }


    private static async Task ProcessTasksConcurrently(int noOfMsgsPerIteration, IActorTest proxy, string id) {
      var tasks = CreateOverlappingTasks(noOfMsgsPerIteration, proxy, id);

      while (tasks.Any()) {
        Task result = null;
        try {
          result = await Task.WhenAny(tasks);
        }
        catch (OperationCanceledException) {
          Console.WriteLine("Timeout..");
        }
        finally {
          tasks.Remove(result);
        }
      }
    }

    private static List<Task> CreateOverlappingTasks(int noOfMsgsPerIteration, IActorTest proxy, string id) {
      return Enumerable
        .Range(0, noOfMsgsPerIteration)
        .Select(i => CreateAndRunTask(proxy, id))
        .ToList();
    }

    private static int GetSequentialOrConcurrentProcessing() {
      Console.WriteLine("1-sequential or 2-concurrent? (default 1-sequential)");
      int option;
      if (!int.TryParse(Console.ReadLine(), out option)) option = 1;

      return option;
    }

    private static int GetRestPeriodMilliseconds() {
      const int defaultMs = 1000;
      Console.WriteLine($"Time between iterations in milliseconds? (Default {defaultMs})");
      int seconds;
      if (!int.TryParse(Console.ReadLine(), out seconds)) seconds = defaultMs;
      return seconds;
    }

    private static int GetIterations() {
      const int defaultIterations = 50;
      Console.WriteLine($"How many messages per iteration? (Default {defaultIterations})");
      int noOfMsgsPerIteration;
      if (!int.TryParse(Console.ReadLine(), out noOfMsgsPerIteration)) noOfMsgsPerIteration = defaultIterations;
      return noOfMsgsPerIteration;
    }

    private static IActorCreator CreateProxy(Uri serviceUri, string id) {
      return ServiceProxy.Create<IActorCreator>(serviceUri, new ServicePartitionKey(1));
    }


    private static T CreateProxy<T>(Uri serviceUri, string id) where T : IActorTest {
      var actorId = new ActorId(id);

      return ActorProxy.Create<T>(actorId, serviceUri);
    }

    private static async Task CreateAndRunTask(IActorTest proxy, string id) {
      var command = CreateIngestCommand(proxy, new LoadTestMsg(id));
      await command.RunAsync();
    }

    private static async Task CreateAndRunCreateActorTask(IActorCreator proxy, string id) {
      var command = CreateIngestCommand2(proxy, new LoadTestMsg(id));
      await command.RunAsync();
    }

    public static void DumpMetricsToConsole() {
      Console.WriteLine($"ActorsCreated:{created} ActorCreateTimeout:{timedout} ActorCreateException:{exception}");

      var circuitBreakers = ResilientCommandCircuitBreaker.Factory.GetAll();
      var metrics = ResilientCommandMetrics.Factory.GetAll();
      metrics
            .Select(cmdMetric => new
            {
              CommandName = cmdMetric.Key,
              CircuitBreakerStatus = circuitBreakers[cmdMetric.Key].IsOpen() ? "OPEN" : "CLOSED",
              Snapshot = cmdMetric.Value.GetStatsSnapshot(),
              //RequestCountBuckets = cmdMetric.Value.GetStatsSnapshot().RequestBuckets.Select(c=>c.SuccessCount),
              Latency_90th = cmdMetric.Value.CalculateLatencyValueAtPercentile(90),
              Latency_99th = cmdMetric.Value.CalculateLatencyValueAtPercentile(99),
              Latency_99_5th = cmdMetric.Value.CalculateLatencyValueAtPercentile(99.5m),
            })
            .ToList()
            .ForEach(result => Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented)));
    }

    public static AbstractResilientCommand<object> CreateIngestCommand(IActorTest proxy, LoadTestMsg msg) {
      return new ResilientCommandWrap<object>(
        "LoadGenIngestCommand",
        async () => {
          await proxy.Ingest(msg);
          return Task.FromResult(1);
        });
    }

    public static AbstractResilientCommand<object> CreateIngestCommand2(IActorCreator proxy, LoadTestMsg msg) {
      return new ResilientCommandWrap<object>(
        "CreateActorCommand",
        async () => {
          await proxy.CreateActor(msg);
          return Task.FromResult(1);
        });
    }

    static UInt64 ComputeFnv1a(byte[] data) {
      UInt64 fnv_offset_basis_64 = 14695981039346656037;
      UInt64 fnv_prime_64 = 1099511628211;
      UInt64 hash = fnv_offset_basis_64;

      foreach (byte byte_of_data in data) {
        hash ^= byte_of_data;
        hash *= fnv_prime_64;
      }

      return hash;
    }
  }
}
