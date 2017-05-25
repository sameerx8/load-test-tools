using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActorDestroyer.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ActorDestroyer {
  /// <summary>
  /// An instance of this class is created for each service instance by the Service Fabric runtime.
  /// </summary>
  internal sealed class ActorDestroyer : StatelessService, IActorDestroyer {
    public ActorDestroyer(StatelessServiceContext context)
        : base(context) { }

    /// <summary>
    /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
    /// </summary>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners() {
      return new ServiceInstanceListener[0];
    }

    /// <summary>
    /// This is the main entry point for your service instance.
    /// </summary>
    /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
    protected override async Task RunAsync(CancellationToken cancellationToken) {
      while (true) {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
      }
    }

    

    public async Task DestroyAsync(string id) {
      var actorId = GetActorId(id);
      var actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/ActorLoadTest/ActorTestActorService"), actorId);
      await actorServiceProxy.DeleteActorAsync(actorId, CancellationToken.None);
    }

    private static ActorId GetActorId(string id) {
      return new ActorId(id);
    }
  }
}
