import { TestBed, async, inject } from '@angular/core/testing';
import { CommandMetricStreamService } from '../services/command-metric-stream.service';

describe('Command Metric Stream Service Tests', () => {

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CommandMetricStreamService
      ]
    });
  });

  it('true is true', () => expect(true).toBe(true));

  it('test1', () => {
    inject([CommandMetricStreamService], (commandMetricService) => {
        expect(commandMetricService).toBeDefined();
    });
  });
  
});
