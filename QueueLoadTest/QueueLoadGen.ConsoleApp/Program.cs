using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Client;
using QueueLoad.Interfaces;
using Resiliency;
using Resiliency.CircuitBreaker;
using Resiliency.Command;
using Resiliency.Metrics;
using Newtonsoft.Json;

namespace QueueLoadGen.ConsoleApp {
  class Program {
    static void Main(string[] args) {
      RunAsync().GetAwaiter().GetResult();
    }

    const string ServiceUri = "fabric:/QueueLoadTest/QueueLoad";
    static async Task RunAsync() {
      var noOfMsgsPerIteration = GetIterations();
      var seconds = GetRestPeriodMilliseconds();
      var sequentialOrConcurrent = GetSequentialOrConcurrentProcessing();

      var restTime = TimeSpan.FromMilliseconds(seconds);

      int messageCount = 0;

      Task.Factory.StartNew(async () => {
        while (true) {
          DumpMetricsToConsole();
          await Task.Delay(1000);
        }
      }, TaskCreationOptions.LongRunning);

      while (true) {
        var id = Guid.NewGuid().ToString();
        var partitionKey = ComputeFnv1A(id);

        var proxy = CreateProxy<IQueueLoadGen>(new Uri(ServiceUri), partitionKey);

        
        Console.WriteLine("Sending");

        if (sequentialOrConcurrent == 1) {
          ProcessTasksSequentially(noOfMsgsPerIteration, proxy, id);
        }
        else {
          await ProcessTasksConcurrently(noOfMsgsPerIteration, proxy, id);
          
        }

        messageCount += noOfMsgsPerIteration;

        Console.WriteLine($"Sent {noOfMsgsPerIteration} messages. SlidingTotal {messageCount}.");

        await Task.Delay(restTime);
      }
    }

    private static void ProcessTasksSequentially(int noOfMsgsPerIteration, IQueueLoadGen proxy, string id) {
      for (int i = 0; i < noOfMsgsPerIteration;i++) {
        try {
          CreateAndRunTask(proxy, id).Wait();
        }
        catch (AggregateException ex) {
          if (ex.InnerException is TaskCanceledException) {
            //timeout
          }
        }
      }
    }

    private static async Task ProcessTasksConcurrently(int noOfMsgsPerIteration, IQueueLoadGen proxy, string id) {
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

    private static List<Task> CreateOverlappingTasks(int noOfMsgsPerIteration, IQueueLoadGen proxy, string id) {
      return Enumerable
        .Range(0, noOfMsgsPerIteration)
        .Select( i => CreateAndRunTask(proxy, id))
        .ToList();
    }

    private static long ComputeFnv1A(string id) {
      return (long)ComputeFnv1a(Encoding.UTF8.GetBytes(id));
    }

    private static int GetSequentialOrConcurrentProcessing() {
      Console.WriteLine("1-sequential or 2-concurrent? (default 1-sequential)");
      int option;
      if (!int.TryParse(Console.ReadLine(), out option)) option = 1;

      return option;
    }

    private static int GetRestPeriodMilliseconds() {
      Console.WriteLine("Time between iterations in milliseconds? (Default 500)");
      int seconds;
      if (!int.TryParse(Console.ReadLine(), out seconds)) seconds = 500;
      return seconds;
    }

    private static int GetIterations() {
      Console.WriteLine("How many messages per iteration? (Default 50)");
      int noOfMsgsPerIteration;
      if (!int.TryParse(Console.ReadLine(), out noOfMsgsPerIteration)) noOfMsgsPerIteration = 50;
      return noOfMsgsPerIteration;
    }

    private static T CreateProxy<T>(Uri serviceUri, long partitionKey) where T: IService {
      var transportSettings = new FabricTransportRemotingSettings
      {
        MaxConcurrentCalls = 32
      };

      ServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory(transportSettings));

      return serviceProxyFactory.CreateServiceProxy<T>(serviceUri, new ServicePartitionKey(partitionKey));
    }

    private static async Task CreateAndRunTask(IQueueLoadGen proxy, string id) {
      var command = CreateIngestCommand(proxy, new QueueLoadTestMessage(id));
      await command.RunAsync();
    }

    public static void DumpMetricsToConsole() {
      var circuitBreakers = ResilientCommandCircuitBreaker.Factory.GetAll();
      var metrics = ResilientCommandMetrics.Factory.GetAll();
      metrics
            .Select(cmdMetric => new {
              CommandName = cmdMetric.Key,
              CircuitBreakerStatus = circuitBreakers[cmdMetric.Key].IsOpen() ? "OPEN" : "CLOSED",
              Snapshot = cmdMetric.Value.GetStatsSnapshot(),
              Latency_90th = cmdMetric.Value.CalculateLatencyValueAtPercentile(90),
              Latency_99th = cmdMetric.Value.CalculateLatencyValueAtPercentile(99),
              Latency_99_5th = cmdMetric.Value.CalculateLatencyValueAtPercentile(99.5m),
            })
            .ToList()
            .ForEach(result => Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented)));
    }

    public static AbstractResilientCommand<object> CreateIngestCommand(IQueueLoadGen proxy, QueueLoadTestMessage msg) {
      return new ResilientCommandWrap<object>(
        "LoadGenIngestCommand",
        async () => {
          await proxy.Ingest(msg);
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
