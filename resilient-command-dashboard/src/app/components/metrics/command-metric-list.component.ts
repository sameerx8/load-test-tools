import { Component, OnInit } from '@angular/core';
import { MetricSnapshotMessage } from './models/metric-snapshot';
import { CommandMetricStreamService } from '../../services/command-metric-stream.service';
import { MetricData } from '../../components/data/metric-data';

@Component({
  selector: 'command-metrics',
  templateUrl: 'command-metric-list.template.html',
  providers: [ CommandMetricStreamService ]
})
export class CommandMetricsList implements OnInit {
  private commandMetrics: MetricSnapshotMessage[] = [];

  public sparklineData:Array<any> = [5, 6, 7, 2, 0, 4, 2, 4, 5, 7, 2, 4, 12, 14, 4, 2, 14, 12, 7];

  public sparklineOptions:any = {
    type: 'line',
    width: '100%',
    height: '40',
    lineColor: '#1ab394',
    fillColor: '#ffffff'
  };

  ngOnInit(): void {
    this.commandMetrics = MetricData;
    if (!this.metricService.metricStream) {
      return;
    }

    this.metricService.metricStream.subscribe(ev => {
       const resultIdx = this.commandMetrics.findIndex(c => c.commandName === ev.commandName);

        if(resultIdx === -1) {
          this.commandMetrics.push(ev);
        } else {
          this.commandMetrics[resultIdx] = ev;
        }
    });
  }

  constructor(private metricService: CommandMetricStreamService) {}
}
