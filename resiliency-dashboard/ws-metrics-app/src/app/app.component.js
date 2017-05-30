"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
require("rxjs/add/observable/of");
require("rxjs/add/operator/debounceTime");
var subject_1 = require("rxjs/subject");
var AppComponent = (function () {
    function AppComponent() {
        this.commandMetrics = [];
        this.url = 'ws://localhost:5000/ws';
    }
    AppComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.ws = new WebSocket(this.url);
        this.sub = new subject_1.Subject();
        this.sub
            .subscribe(function (ev) {
            var resultIdx = _this.commandMetrics.findIndex(function (c) { return c.commandName === ev.commandName; });
            if (resultIdx === -1) {
                _this.commandMetrics.push(ev);
            }
            else {
                _this.commandMetrics[resultIdx] = ev;
            }
        });
        this.ws.onopen = function (evt) {
            console.log('connected to ' + _this.url);
        };
        this.ws.onclose = function (ev) {
            console.log('closing web socket. reason:' + ev.reason);
        };
        this.ws.onmessage = function (evt) {
            console.log(evt.data);
            console.log(JSON.parse(evt.data));
            _this.sub.next(JSON.parse(evt.data));
        };
    };
    return AppComponent;
}());
AppComponent = __decorate([
    core_1.Component({
        selector: 'my-app',
        template: "\n  <h1>Resilient Commands Circuit Status</h1>\n    <ul>\n      <li *ngFor=\"let commandMetric of commandMetrics\">\n        <div class=\"command\">\n          <h3>{{commandMetric.commandName}}</h3>\n          <p>Hosts: {{commandMetric.hostName}} </p>\n          <p>Latency_90th: {{commandMetric.latency_90th}}</p>\n          <p>Latency_99th: {{commandMetric.latency_99th}}</p>\n          <p>Latency_99_5th: {{commandMetric.latency_99_5th}}</p>\n          <p>MessagesPerSecond: {{commandMetric.messagesPerSecond}}</p>\n          <p><strong>Circuit Breaker Status: {{commandMetric.circuitBreakerStatus}}</strong></p>\n\n          <ul>\n            <li *ngFor=\"let eventType of commandMetric.aggregates\">\n              <span>{{eventType.eventTypeName}}:{{eventType.total}}</span>\n            </li>\n          </ul>\n        </div>\n      </li>\n    </ul>\n  ",
        styles: ["\n\n.command {\n  border: 1px solid #ffffff;\n}\n\n\n  "]
    })
], AppComponent);
exports.AppComponent = AppComponent;
//# sourceMappingURL=app.component.js.map