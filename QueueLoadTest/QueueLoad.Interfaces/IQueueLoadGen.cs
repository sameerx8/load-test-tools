using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace QueueLoad.Interfaces {
  public interface IQueueLoadGen : IService  {
    Task Ingest(QueueLoadTestMessage msg);
  }
}