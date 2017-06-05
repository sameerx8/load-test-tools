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
  private requestRates: CommandRequestRate[] = [] ;

  public sparklineData:Array<any> = [5, 6, 7, 2, 0, 4, 2, 4, 5, 7, 2, 4, 12, 14, 4, 2, 14, 12, 7];

  public sparklineOptions:any = {
    type: 'line',
    width: '100%',
    height: '40',
    lineColor: '#1ab394',
    fillColor: '#ffffff'
  };

  ngOnInit(): void {
    this.metricService.connect();

    if (!this.metricService.metricStream) {
      this.commandMetrics = MetricData;
      return;
    }

    this.metricService
    .metricStream
    .subscribe(ev => {
       const resultIdx = this.commandMetrics.findIndex(c => c.commandName === ev.commandName);
       const requestRateIdx = this.requestRates.findIndex(c => c.commandName === ev.commandName);

        if (requestRateIdx === -1) {
          const rate: CommandRequestRate = {
            commandName: ev.commandName,
            requestRates: [ev.messagesPerSecond]
          };

          this.requestRates.push(rate);
          ev.requestRate = rate.requestRates;

        } else {
          if(this.requestRates[requestRateIdx].requestRates.length > 120) {
            this.requestRates[requestRateIdx].requestRates.shift();
          }
          this.requestRates[requestRateIdx].requestRates.push(ev.messagesPerSecond)

          ev.requestRate = this.requestRates[requestRateIdx].requestRates;
        }

        if(resultIdx === -1) {
          this.commandMetrics.push(ev);
        } else {
          this.commandMetrics[resultIdx] = ev;
        }

    });
  }

  constructor(private metricService: CommandMetricStreamService) {}
}


export class CommandRequestRate {
  commandName : string;
  requestRates: Array<number>;
}