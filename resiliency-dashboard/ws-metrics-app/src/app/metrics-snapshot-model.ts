export class MetricSnapshotMessage{
  commandName: string;
  aggregates: EventTypeAggregate[];
  latency_90th: number;
  latency_99th: number;
  latency_99_5th: number;
  messagesPerSecond: number;
  circuitBreakerStatus: string;
  hostName: string;
}

export class EventTypeAggregate{
  eventTypeName: string;
  total: number;
}