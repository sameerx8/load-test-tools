using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActorCreator.Interfaces;
using ActorTest.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Resiliency;
using Resiliency.CircuitBreaker;
using Resiliency.Command;
using Resiliency.Metrics;

namespace ActorCreator {
  /// <summary>
  /// An instance of this class is created for each service replica by the Service Fabric runtime.
  /// </summary>
  internal sealed class ActorCreator : StatefulService, IActorCreator {
    public ActorCreator(StatefulServiceContext context)
        : base(context) { }

    /// <summary>
    /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
    /// </summary>
    /// <remarks>
    /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
    /// </remarks>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() {
      var listenerSettings = new FabricTransportRemotingListenerSettings { MaxConcurrentCalls = 32 };
      return new[] { new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this, listenerSettings), "ActorLoadTest") };
    }

    private readonly List<IModel> _consumerChannels = new List<IModel>();

    /// <summary>
    /// This is the main entry point for your service replica.
    /// This method executes when this replica of your service becomes primary and has write status.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken) {
      var connection = CreateConnection("sameersp3", "admin", "admin");
      Enumerable
        .Range(0, 5)
        .ToList()
        .ForEach(id => _consumerChannels.Add(CreateConsumerChannel(connection, $"LoadTestMsg-Consumer-{id}")));

      while (true) {
        cancellationToken.ThrowIfCancellationRequested();

        DumpMetricsToConsole();

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
      }
    }

    private int _actorsCreated;
    private IModel CreateConsumerChannel(IConnection connection, string id) {
      var channel = connection.CreateModel();
      channel.BasicQos(0, 10, true);

      var consumer = new EventingBasicConsumer(channel);

      consumer.Received += async (sender, args) => {
        var msg = JsonConvert.DeserializeObject<LoadTestMsg>( Encoding.UTF8.GetString(args.Body));
        var actorId = new ActorId(msg.Id);
        var actorProxy = ActorProxy.Create<IActorTest>(actorId, new Uri("fabric:/ActorLoadTest/ActorTestActorService"));

        var deliveryTag = args.DeliveryTag;
        try {

          await CreateAndRunTask(actorProxy, msg);
          Interlocked.Increment(ref _actorsCreated);
        }
        catch (Exception ex) {

        }
        finally {
          channel.BasicAck(deliveryTag, false);
        }
        
      };

      channel.BasicConsume("sameer-test-1", false, id, consumer);

      return channel;
    }

    private IConnection CreateConnection(string hostName, string userName, string password) {
      var factory = new ConnectionFactory {
        UserName = userName,
        Password = password,
        HostName = hostName,
        VirtualHost = "/"
      };

      return factory.CreateConnection();
    }

    private async Task CreateAndRunTask(IActorTest proxy, LoadTestMsg msg) {
      var command = CreateIngestCommand(proxy, msg);
      await command.RunAsync();
    }

    public void DumpMetricsToConsole() {

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
            .ForEach(result => ServiceEventSource.Current.Message($"ActorsCreated: {_actorsCreated} Metrics: { JsonConvert.SerializeObject(result, Formatting.Indented) }"));
    }

    private AbstractResilientCommand<object> CreateIngestCommand(IActorTest proxy, LoadTestMsg msg) {
      return new ResilientCommandWrap<object>(
        "LoadGenIngestCommand",
        async () => {
          await proxy.Ingest(msg);
          Interlocked.Increment(ref _actorsCreated);
          return Task.FromResult(1);
        });
    }

    const string ServiceUri = "fabric:/ActorLoadTest/ActorTestActorService";
    private static T CreateProxy<T>(Uri serviceUri, LoadTestMsg msg) where T : IActorTest {
      var actorId = new ActorId(msg.Id);

      return ActorProxy.Create<T>(actorId, serviceUri);
    }

    public async Task CreateActor(LoadTestMsg msg) {
      var proxy = CreateProxy<IActorTest>(new Uri(ServiceUri), msg);

      await CreateAndRunTask(proxy, msg);
    }
  }
}
