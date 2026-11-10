import type { App } from 'vue';
import VxeTable from 'vxe-table';
import 'vxe-table/lib/style.css';

export function installTable(app: App): void {
  app.use(VxeTable);
}
