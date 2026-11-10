<script setup lang="ts">
import { onActivated, ref } from 'vue';
import type { VxeTableInstance } from 'vxe-table';

interface Row {
  id: number;
  number: string;
  amount: string;
}

const rows: Row[] = Array.from({ length: 2000 }, (_, i) => ({
  id: i + 1,
  number: `SP-${String(i + 1).padStart(5, '0')}`,
  amount: (i * 13.7).toFixed(2),
}));

const table = ref<VxeTableInstance<Row>>();
const recalculateOnActivate = new URLSearchParams(location.search).get('recalc') !== '0';

// 失活期间窗口尺寸可能变化，激活时让表格重新计算布局。
onActivated(async () => {
  if (recalculateOnActivate) {
    await table.value?.recalculate();
  }
});

defineExpose({ table });
</script>

<template>
  <vxe-table
    ref="table"
    data-testid="sample-table"
    :data="rows"
    height="400"
    border
    :row-config="{ keyField: 'id', isHover: true }"
    :virtual-y-config="{ enabled: true, gt: 100 }"
    :column-config="{ resizable: true }"
  >
    <vxe-column field="number" title="单号" fixed="left" width="160" />
    <vxe-column field="amount" title="金额" min-width="200" />
    <vxe-column field="id" title="序号" width="120" />
  </vxe-table>
</template>
