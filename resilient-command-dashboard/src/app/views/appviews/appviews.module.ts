import {NgModule} from "@angular/core";
import {BrowserModule} from "@angular/platform-browser";
import {RouterModule} from "@angular/router";
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {PeityModule } from '../../components/charts/peity';
import {SparklineModule } from '../../components/charts/sparkline';
import {CommandMetricsComponent} from '../../components/metrics/command-metrics.component';
import {CommandMetricStreamService} from '../../services/command-metric-stream.service';
import {CommandFilterPipe} from '../../pipes/command-filter.pipe';
import {CommandOrderPipe} from '../../pipes/command-sort.pipe';

@NgModule({
  declarations: [
    CommandMetricsComponent,
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
    CommandMetricsComponent
  ],
  providers: [CommandMetricStreamService]
})

export class AppviewsModule { }
