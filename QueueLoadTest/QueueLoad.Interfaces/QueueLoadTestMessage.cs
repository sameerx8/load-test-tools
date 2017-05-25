using System.Runtime.Serialization;

namespace QueueLoad.Interfaces {
  [DataContract]
  public class QueueLoadTestMessage {
    public QueueLoadTestMessage(string messageId) {
      MessageId = messageId;
    }
    [DataMember]
    public string MessageId { get; private set; }
  }
}