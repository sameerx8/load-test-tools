"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var core_1 = require("@angular/core");
var subject_1 = require("rxjs/subject");
require("rxjs/add/operator/share");
var MetricsFeed = (function () {
    function MetricsFeed() {
        this.url = 'ws://localhost:5000/ws';
    }
    MetricsFeed.prototype.getObservable = function () {
        var _this = this;
        this.ws = new WebSocket(this.url);
        this.sub = new subject_1.Subject();
        this.ws.onmessage = function (evt) {
            console.log(evt.data);
            console.log(JSON.parse(evt.data));
            _this.sub.next(evt.data);
        };
        return this.sub.asObservable().share();
    };
    return MetricsFeed;
}());
MetricsFeed = __decorate([
    core_1.Injectable()
], MetricsFeed);
exports.MetricsFeed = MetricsFeed;
//# sourceMappingURL=metrics-feed.service.js.map