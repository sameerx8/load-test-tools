import { MetricSnapshotMessage } from '../components/metrics/models/metric-snapshot';
import { CommandOrderPipe } from './command-sort.pipe';
import { MetricData } from '../components/data/metric-data';

describe('Command filter pipe tests', () => {
  let pipe: CommandOrderPipe;

  beforeEach(() => {
    pipe = new CommandOrderPipe();
  });

  it('should be defined', () => {
    expect(pipe).toBeDefined();
  });

});