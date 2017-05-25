using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

namespace ActorTest {
  public class LoadActorService : ActorService {
    public LoadActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings) {}

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() {
      var listenerSettings = new FabricTransportRemotingListenerSettings { MaxConcurrentCalls = 32 };
      return new[] { new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this, listenerSettings), "ActorLoadTest") };
    }

  }
}
