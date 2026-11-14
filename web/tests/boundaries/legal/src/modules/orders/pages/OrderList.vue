<script setup lang="ts">
import { Row } from 'antdv-next';
import type { OrdersApi } from '@/modules/orders/api';
import type { Paths } from '@/generated/api';
import type { Client } from '@/platform/api';

defineProps<{ api: OrdersApi; paths: Paths; client: Client }>();
</script>

<template>
  <Row>订单列表</Row>
</template>
