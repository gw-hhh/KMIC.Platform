import type { Paths } from '@/generated/api';
import type { OrdersApi } from '@/modules/orders/api';
import type { Client } from '@/platform/api';
import Shell from '@/shell/Shell.vue';

export { Shell };
export type Wired = { paths: Paths; orders: OrdersApi; client: Client };
