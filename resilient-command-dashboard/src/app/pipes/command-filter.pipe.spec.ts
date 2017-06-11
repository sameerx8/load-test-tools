import { MetricSnapshotMessage } from '../components/metrics/models/metric-snapshot';
import { CommandFilterPipe } from './command-filter.pipe';
import { MetricData } from '../components/data/metric-data';
 
describe('Command filter pipe tests', () => {
  let pipe: CommandFilterPipe;

  beforeEach(() => {
    pipe = new CommandFilterPipe();
  });

  it('should be defined', () => {
    expect(pipe).toBeDefined();
  });

  it('given null should return array as is.', () => {
    const result = pipe.transform(MetricData, null);
    expect(result).toBe(MetricData);
  });

  it('given empty filter should return array as is.', () => {
    const result = pipe.transform(MetricData, '');
    expect(result).toBe(MetricData);
  });

  it('given whitespace filter should return empty result', () => {
    const result = pipe.transform(MetricData, '  ');
    expect(result.length).toEqual(0);
  });

  it('given filter with no matches should return empty result', () => {
    const result = pipe.transform(MetricData, 'bob');
    expect(result.length).toEqual(0); 
  });

  it('given filter with single match should return 1 result', () => {
    const result = pipe.transform(MetricData, 'test2');
    expect(result.length).toEqual(1);
  });

  it('given filter with 2 matches should return 2 results', () => {
    const result = pipe.transform(MetricData, 'test');
    expect(result.length).toEqual(2);
  });



});
