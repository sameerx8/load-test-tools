using ActorTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ActorCreator.Interfaces {
  public interface IActorCreator : IService {
    Task CreateActor(LoadTestMsg msg);
  }
}
