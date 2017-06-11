import { Pipe, PipeTransform } from '@angular/core';
import { MetricSnapshotMessage } from '../components/metrics/models/metric-snapshot';

@Pipe({
  name: 'commandOrder'
})
export class CommandOrderPipe implements PipeTransform {
    static _orderByComparator(a:any, b:any): number {
      if ((isNaN(parseFloat(a)) || !isFinite(a)) || (isNaN(parseFloat(b)) || !isFinite(b))) {
        // Isn't a number so lowercase the string to properly compare
        if (a.toLowerCase() < b.toLowerCase()) {
          return -1;
        }

        if (a.toLowerCase() > b.toLowerCase()) {
          return 1;
        }
      } else {
        // Parse strings as numbers to compare properly
        if (parseFloat(a) < parseFloat(b)) {
          return -1;
        }
        if (parseFloat(a) > parseFloat(b)) {
          return 1;
        }
      }

      return 0; // equal each other
    }

  transform(items: any[], sortKey: any, asc: boolean) {
    if (!sortKey) {
      return items;
    }

    const result = items.sort((a: any, b: any) => {
      return !asc
        ? CommandOrderPipe._orderByComparator(a[sortKey], b[sortKey]) 
        : -CommandOrderPipe._orderByComparator(a[sortKey], b[sortKey]);
    });

    return result;
  }
}