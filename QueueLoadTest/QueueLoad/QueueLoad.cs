using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using QueueLoad.Interfaces;

namespace QueueLoad {
  /// <summary>
  /// An instance of this class is created for each service replica by the Service Fabric runtime.
  /// </summary>
  internal sealed class QueueLoad : StatefulService, IQueueLoadGen {
    public QueueLoad(StatefulServiceContext context)
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
      return new[] { new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this, listenerSettings), "Queue Load Gen Endpoint") };
    }

    /// <summary>
    /// This is the main entry point for your service replica.
    /// This method executes when this replica of your service becomes primary and has write status.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken) {
      try {
        while (true) {
          cancellationToken.ThrowIfCancellationRequested();

          await ReportQueueDepth(cancellationToken);

          var dequeueingTasks = Enumerable.Range(0, 5)
            .Select(i => DequeueItemAndProcess(cancellationToken));

          await Task.WhenAll(dequeueingTasks);

          await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
      }
      catch (Exception ex) {
        ServiceEventSource.Current.Message("Exception occurred: {0}", ex);
        throw;
      }
    }

    private const int BatchSize = 1000;

    public async Task DequeueItemAndProcess(CancellationToken token) {
      var queue = await GetReliableQueue();

      using (var tx = StateManager.CreateTransaction()) {
        for (int i = 0; i < BatchSize; i++) {
          if (queue.Count == 0) break;
          await queue.TryDequeueAsync(tx, token);
        }
        await tx.CommitAsync();
      }
    }

    const string QueueName = "testqueue";

    public async Task ReportQueueDepth(CancellationToken token) {
      var queue = await GetReliableQueue();
      int value = (int)queue.Count;
      Partition.ReportLoad(new List<LoadMetric> { new LoadMetric("QueueLength", value) });
    }

    private async Task<IReliableConcurrentQueue<QueueLoadTestMessage>> GetReliableQueue() {
      return await StateManager.GetOrAddAsync<IReliableConcurrentQueue<QueueLoadTestMessage>>(QueueName);
    }

    public async Task Ingest(QueueLoadTestMessage msg) {

      var queue = await StateManager.GetOrAddAsync<IReliableConcurrentQueue<QueueLoadTestMessage>>(QueueName);
      using (var tx = StateManager.CreateTransaction()) {
        await queue.EnqueueAsync(tx, msg);
        await tx.CommitAsync();
      }
    }
  }
}
