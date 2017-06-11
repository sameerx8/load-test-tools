import { Pipe, PipeTransform } from '@angular/core';
import { MetricSnapshotMessage } from '../components/metrics/models/metric-snapshot';

@Pipe({
  name: 'commandFilter'
})
export class CommandFilterPipe implements PipeTransform {
  transform(items: MetricSnapshotMessage[], filter: string) {
    if (!filter) {
      return items;
    }

    return items.filter(command => command.commandName.toLowerCase().includes(filter) || command.commandTag.toLowerCase().includes(filter));
  }
}
