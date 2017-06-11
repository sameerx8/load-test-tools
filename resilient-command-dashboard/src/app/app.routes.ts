import {Routes} from "@angular/router";

import {BlankLayoutComponent} from "./components/common/layouts/blankLayout.component";
import {BasicLayoutComponent} from "./components/common/layouts/basicLayout.component";
import {TopNavigationLayoutComponent} from "./components/common/layouts/topNavigationlayout.component";

import {CommandMetricsComponent} from './components/metrics/command-metrics.component';

export const ROUTES:Routes = [
  // Main redirect
  {path: '', redirectTo: 'dashboard', pathMatch: 'full'},

  // App views
  {
    path: '', component: BlankLayoutComponent,
    children: [
      {path: 'dashboard', component: CommandMetricsComponent}
    ]
  },
  // Handle all other routes
  {path: '**',  redirectTo: 'dashboard'}
];
