import {MetricSnapshotMessage} from '../metrics/models/metric-snapshot';

export const MetricData : MetricSnapshotMessage[] = [
  {commandName: 'test', latency_90th: 10, latency_99th: 20, latency_99_5th: 100, messagesPerSecond: 44.3, circuitBreakerStatus: "CLOSED", hostName: "fpda" },
  {commandName: 'test2', latency_90th: 104, latency_99th: 220, latency_99_5th: 12100, messagesPerSecond: 4334.3, circuitBreakerStatus: "CLOSED", hostName: "fpda" },
]


  // commandName: string;
  // latency_90th: number;
  // latency_99th: number;
  // latency_99_5th: number;
  // messagesPerSecond: number;
  // circuitBreakerStatus: string;
  // hostName: string;