import { Component, OnInit } from '@angular/core';
import { MetricsFeed} from './metrics-feed.service';
import { Observable } from 'rxjs/Observable';
import { MetricSnapshotMessage } from './metrics-snapshot-model';

import 'rxjs/add/observable/of';
import 'rxjs/add/operator/debounceTime';

import { Subject } from 'rxjs/subject';

@Component({
  selector: 'my-app',
  template: `
  <h1>Resilient Commands Circuit Status</h1>
    <ul>
      <li *ngFor="let commandMetric of commandMetrics">
        <div class="command">
          <h3>{{commandMetric.commandName}}</h3>
          <p>Hosts: {{commandMetric.hostName}} </p>
          <p>Latency_90th: {{commandMetric.latency_90th}}</p>
          <p>Latency_99th: {{commandMetric.latency_99th}}</p>
          <p>Latency_99_5th: {{commandMetric.latency_99_5th}}</p>
          <p>MessagesPerSecond: {{commandMetric.messagesPerSecond}}</p>
          <p><strong>Circuit Breaker Status: {{commandMetric.circuitBreakerStatus}}</strong></p>

          <ul>
            <li *ngFor="let eventType of commandMetric.aggregates">
              <span>{{eventType.eventTypeName}}:{{eventType.total}}</span>
            </li>
          </ul>
        </div>
      </li>
    </ul>
  `,
  styles: [`

.command {
  border: 1px solid #ffffff;
}


  `]
})
export class AppComponent implements OnInit {
  private commandMetrics: MetricSnapshotMessage[] = [];
  private subscriber: Observable<MetricSnapshotMessage>;
  private ws: WebSocket;
  private url='ws://localhost:5000/ws';
  private sub: Subject<MetricSnapshotMessage>;

  ngOnInit(): void {
    this.ws = new WebSocket(this.url);
    this.sub = new Subject<MetricSnapshotMessage>();

    this.sub
    .subscribe(
      ev => {
        let resultIdx = this.commandMetrics.findIndex(c => c.commandName === ev.commandName);

        if(resultIdx === -1) {
          this.commandMetrics.push(ev);
        } else {
          this.commandMetrics[resultIdx] = ev;
        }
      });

    this.ws.onopen = (evt: MessageEvent) => {
      console.log('connected to ' + this.url);
    }

    this.ws.onclose = (ev: CloseEvent) => {
      console.log('closing web socket. reason:' + ev.reason);
    }

    this.ws.onmessage = (evt: MessageEvent) => {
      console.log(evt.data);
      console.log(JSON.parse(evt.data));
      this.sub.next(JSON.parse(evt.data));
    }
  }
}
