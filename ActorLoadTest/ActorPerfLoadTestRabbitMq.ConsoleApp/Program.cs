using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActorTest.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Resiliency;
using Resiliency.CircuitBreaker;
using Resiliency.Command;
using Resiliency.Metrics;

namespace ActorPerfLoadTestRabbitMq.ConsoleApp {
  class Program {
    static void Main(string[] args) {
      var factory = new ConnectionFactory
      {
        HostName = "sameersp3",
        UserName = "admin",
        Password = "admin",
        VirtualHost = "/"
      };

      var connection = factory.CreateConnection();

      var models = new List<IModel>();

      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));

      StartTiming().GetAwaiter().GetResult();

      Console.Read();

    }

    private static async Task StartTiming() {
      while (true) {
        Console.WriteLine($"Consumed:{received} ActorsCreated:{created} ActorCreateTimeout:{timeout} ActorCreateException:{exception}");
        DumpMetricsToConsole();
        await Task.Delay(1000);
      }
    }

    private static int received = 0;
    private static int created = 0;
    private static int timeout = 0;
    private static int exception = 0;

    const string ServiceUri = "fabric:/ActorLoadTest/ActorTestActorService";

    public static IModel CreateModel(IConnection connection) {
      var channel = connection.CreateModel();
      var consumer = new EventingBasicConsumer(channel);
      consumer.Received += async (sender, args) =>
      {
        Interlocked.Increment(ref received);
        var msg = JsonConvert.DeserializeObject<LoadTestMsg>(Encoding.UTF8.GetString(args.Body));
        var proxy = CreateProxy<IActorTest>(new Uri(ServiceUri), msg.Id);
        try {
          await CreateAndRunTask(proxy, msg.Id);
          Interlocked.Increment(ref created);
        }
        catch (OperationCanceledException) {
          Interlocked.Increment(ref timeout);
        }
        catch (Exception ex) {
          Interlocked.Increment(ref exception);
        }
        
        channel.BasicAck(args.DeliveryTag, false);

      };
      channel.BasicConsume("sameer-test-1", false, consumer);
      return channel;
    }

    private static T CreateProxy<T>(Uri serviceUri, string id) where T : IActorTest {
      var actorId = new ActorId(id);

      return ActorProxy.Create<T>(actorId, serviceUri);
    }

    private static async Task CreateAndRunTask(IActorTest proxy, string id) {
      var command = CreateIngestCommand(proxy, new LoadTestMsg(id));
      await command.RunAsync();
    }

    public static AbstractResilientCommand<object> CreateIngestCommand(IActorTest proxy, LoadTestMsg msg) {
      return new ResilientCommandWrap<object>(
        "LoadGenIngestCommand",
        async () => {
          await proxy.Ingest(msg);
          return Task.FromResult(1);
        });
    }

    public static void DumpMetricsToConsole() {
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
  }
}
