import { Component, Input } from '@angular/core';
import { MetricSnapshotMessage } from './models/metric-snapshot';

@Component({
  selector: 'command-metric',
  templateUrl: 'powergrid-row.template.html'
})
export class CommandMetric {
  @Input() commandMetric: MetricSnapshotMessage;
  @Input() sparklineData: Array<any>;
  @Input() sparklineOptions: any;
}