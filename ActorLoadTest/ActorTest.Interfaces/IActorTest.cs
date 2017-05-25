using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace ActorTest.Interfaces {
  /// <summary>
  /// This interface defines the methods exposed by an actor.
  /// Clients use this interface to interact with the actor that implements it.
  /// </summary>
  public interface IActorTest : IActor {
    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <returns></returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// TODO: Replace with your own actor method.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    Task SetCountAsync(int count, CancellationToken cancellationToken);

    Task Ingest(LoadTestMsg msg);
  }

  

  [DataContract]
  public class LoadTestMsg {
    public LoadTestMsg(string id) {
      Id = id;
    }

    [DataMember]
    public string Id { get; private set; }
  }
}
