export class MetricSnapshotMessage{
  commandName: string;
  latency_90th: number;
  latency_99th: number;
  latency_99_5th: number;
  messagesPerSecond: number;
  circuitBreakerStatus: string;
  hostName: string;
  requestRate: Array<number>;
}

export class EventTypeAggregate{
  eventTypeName: string;
  total: number;
}