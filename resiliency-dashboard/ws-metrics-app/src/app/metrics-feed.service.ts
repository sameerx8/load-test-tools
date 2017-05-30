import { Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/subject';
import { MetricSnapshotMessage } from './metrics-snapshot-model';

import 'rxjs/add/operator/share';


@Injectable()
export class MetricsFeed {
  private url='ws://localhost:5000/ws';
  private ws: WebSocket;
  private sub: Subject<MetricSnapshotMessage>;

  getObservable() : Observable<MetricSnapshotMessage> {
    this.ws = new WebSocket(this.url);
    this.sub = new Subject<MetricSnapshotMessage>();

    this.ws.onmessage = (evt: MessageEvent) => {
        console.log(evt.data);
        console.log(JSON.parse(evt.data));
        this.sub.next(evt.data);
    }
    return this.sub.asObservable().share();
  }
}



