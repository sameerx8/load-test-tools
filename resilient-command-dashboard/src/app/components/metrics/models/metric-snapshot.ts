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
