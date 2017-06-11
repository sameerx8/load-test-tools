export class MetricSnapshotMessage {
  commandName: string;
  commandTag: string;
  latency_90th: number;
  latency_99th: number;
  latency_99_5th: number;
  messagesPerSecond: number;
  circuitBreakerStatus: string;
  hostName: string;
  success: number;
  failure: number;
  timeout: number;
  retry: number;
  fallbackSuccess: number;
  fallbackFailure: number;
  shortCircuited: number;
  errorPercentage: number;
  requestRate: Array<number>;
  noOfHosts: number;
}

export class EventTypeAggregate{
  eventTypeName: string;
  total: number;
}


  //  public class MetricSnapshot {
  //       public string commandName { get; set; }
  //       public List<EventTypeAggregate> aggregates {get;set;}
  //       public long success{get;set;}
  //       public long failure {get;set;}
  //       public long timeout {get;set;}
  //       public long fallbackSuccess{get;set;}
  //       public long fallbackFailure{get;set;}
  //       public long shortCircuited{get;set;}
  //       public decimal latency_90th { get; set; }
  //       public decimal latency_99th { get; set; }
  //       public decimal latency_99_5th { get; set; }
  //       public decimal messagesPerSecond {get;set;}
  //       public string circuitBreakerStatus{get;set;}
  //       public string hostName{get;set;}
  //   }