import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {RouterModule} from "@angular/router";
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import {StarterViewComponent} from "./starterview.component";
import {LoginComponent} from "./login.component";

import {PeityModule } from '../../components/charts/peity';
import {SparklineModule } from '../../components/charts/sparkline';

import {CommandMetricsList} from '../../components/metrics/command-metric-list.component';
import {CommandMetric} from '../../components/metrics/command-metric.component';

import {CommandMetricStreamService} from '../../services/command-metric-stream.service';

import {CommandFilterPipe} from '../../pipes/command-filter.pipe';
import {CommandOrderPipe} from '../../pipes/command-sort.pipe';

@NgModule({
  declarations: [
    StarterViewComponent,
    LoginComponent,
    CommandMetric,
    CommandMetricsList,
    CommandFilterPipe,
    CommandOrderPipe
  ],
  imports: [
    BrowserModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
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
