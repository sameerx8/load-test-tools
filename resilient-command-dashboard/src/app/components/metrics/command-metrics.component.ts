import { Component, OnInit } from '@angular/core';
import { MetricSnapshotMessage } from './models/metric-snapshot';
import { CommandRequestRate } from './models/command-request-rate';
import { SortState } from './models/sort-state';
import { CommandMetricStreamService } from '../../services/command-metric-stream.service';
import { MetricData } from '../../components/data/metric-data';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/debounceTime';
import { CommandFilterPipe } from '../../pipes/command-filter.pipe';
import { CommandOrderPipe } from '../../pipes/command-sort.pipe';


@Component({
  selector: 'command-metrics',
  templateUrl: 'command-metrics.template.html',
  providers: [ CommandMetricStreamService ]
})
export class CommandMetricsComponent implements OnInit {
  private commandMetrics: MetricSnapshotMessage[] = [];
  private requestRates: CommandRequestRate[] = [] ;
  private sortState: SortState[] = [];
  public searchText = '';
  public sortKey = '';
  public sortAsc = true;
  public searchInputCtrl = new FormControl();

  public sparklineOptions: any = {
    type: 'line',
    width: '100%',
    height: '40',
    lineColor: '#1ab394',
    fillColor: '#ffffff'
  };

  public setSortKey(key: string) {
    const keyIdx = this.sortState.findIndex(c => c.sortKey === key);

    if (keyIdx === -1) {
      const sortState: SortState = {
        sortKey: key,
        asc: true
      };

      this.sortState.push(sortState);
    } else {
      this.sortState[keyIdx].asc = !this.sortState[keyIdx].asc;
    }

    this.sortKey = key;
    this.sortAsc = this.sortState[keyIdx] ? this.sortState[keyIdx].asc : true;
  }

  ngOnInit(): void {
    this.searchInputCtrl
      .valueChanges
      .debounceTime(200)
      .subscribe(search => {
        this.searchText = search;
      });

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
          if (this.requestRates[requestRateIdx].requestRates.length > 120) {
            this.requestRates[requestRateIdx].requestRates.shift();
          }
          this.requestRates[requestRateIdx].requestRates.push(ev.messagesPerSecond)

          ev.requestRate = this.requestRates[requestRateIdx].requestRates;
        }

        if (resultIdx === -1) {
          this.commandMetrics.push(ev);
        } else {
          this.commandMetrics[resultIdx] = ev;
        }
    });
  }

  constructor(private metricService: CommandMetricStreamService) {}
}


