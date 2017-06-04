import { Injectable, OnInit } from '@angular/core';
import { MetricData } from '../components/data/metric-data';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { MetricSnapshotMessage } from '../components/metrics/models/metric-snapshot';

@Injectable()
export class CommandMetricStreamService implements OnInit{
  private internalStream: Subject<MetricSnapshotMessage>;
  private url: string;
  private ws: WebSocket;

  public metricStream: Observable<MetricSnapshotMessage>;

  ngOnInit(): void {
    this.url = 'ws://localhost:5000/ws';
    this.ws = new WebSocket(this.url);
    this.internalStream = new Subject<MetricSnapshotMessage>();
    this.metricStream = this.internalStream.asObservable();

    this.ws.onmessage = (ev: MessageEvent) => {
        this.internalStream.next(JSON.parse(ev.data));
    }
  }
}