using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ActorDestroyer.Interfaces {
  public interface IActorDestroyer : IService {
    Task DestroyAsync(string id);
  }
}
