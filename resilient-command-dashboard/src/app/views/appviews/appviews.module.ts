import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {RouterModule} from "@angular/router";

import {StarterViewComponent} from "./starterview.component";
import {LoginComponent} from "./login.component";

import {PeityModule } from '../../components/charts/peity';
import {SparklineModule } from '../../components/charts/sparkline';

import {CommandMetricsList} from '../../components/metrics/command-metric-list.component';
import {CommandMetric} from '../../components/metrics/command-metric.component';

import {CommandMetricStreamService} from '../../services/command-metric-stream.service';

@NgModule({
  declarations: [
    StarterViewComponent,
    LoginComponent,
    CommandMetric,
    CommandMetricsList
  ],
  imports: [
    BrowserModule,
    RouterModule,
    PeityModule,
    SparklineModule
  ],
  exports: [
    StarterViewComponent,
    LoginComponent,
    CommandMetric,
    CommandMetricsList
  ],
  providers: [CommandMetricStreamService]
})

export class AppviewsModule {
}
