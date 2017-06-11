import {MetricSnapshotMessage} from '../metrics/models/metric-snapshot';

export const MetricData: MetricSnapshotMessage[] = [
  {
    commandName: 'test',
    latency_90th: 10,
    latency_99th: 20,
    latency_99_5th: 100,
    messagesPerSecond: 44.3,
    circuitBreakerStatus: 'CLOSED',
    hostName: 'fpda',
    requestRate: null,
    success: 10,
    failure: 20,
    fallbackFailure: 0,
    fallbackSuccess: 0,
    shortCircuited: 0,
    retry: 1,
    commandTag: 'message-enrichment-service',
    timeout: 0,
    noOfHosts: 1,
    errorPercentage: 0
  },
  {
    commandName: 'test2',
    latency_90th: 10,
    latency_99th: 20,
    latency_99_5th: 100,
    messagesPerSecond: 44.3,
    circuitBreakerStatus: 'OPEN',
    hostName: 'fpda',
    requestRate: null,
    success: 10,
    failure: 20,
    fallbackFailure: 0,
    fallbackSuccess: 0,
    shortCircuited: 0,
    retry: 1,
    commandTag: 'coordinator-service',
    timeout: 0,
    noOfHosts: 3,
    errorPercentage: 20
  }
 ];

