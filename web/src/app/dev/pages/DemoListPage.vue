<script setup lang="ts">
import { Button, Card, Input, Space } from 'antdv-next';
import { Permissions } from '@/generated/permissions';
import { useCan } from '@/platform/authz/use-can';
import { useNavigate } from '@/platform/routing';

const navigate = useNavigate();
const canCreate = useCan(Permissions.Sample.Specimen.Create);
const ids = ['SP-001', 'SP-002', 'SP-003'];
// 撑出滚动条，用来看切回标签时滚动位置和焦点能否恢复
const rows = Array.from({ length: 40 }, (_, i) => `示例记录 ${String(i + 1)}`);
</script>

<template>
  <Space direction="vertical" class="demo-list">
    <Card title="示例列表">
      <Space wrap>
        <Button v-for="id in ids" :key="id" @click="navigate('demo.detail', { id })">打开 {{ id }}</Button>
        <Button v-if="canCreate" type="primary" @click="navigate('demo.create')">新建</Button>
      </Space>
    </Card>
    <Card title="示例记录">
      <p v-for="row in rows" :key="row">{{ row }}</p>
      <Input placeholder="滚到底部输入，切到别的标签再切回来" aria-label="底部输入框" />
    </Card>
  </Space>
</template>

<style scoped lang="scss">
.demo-list {
  width: 100%;
}
</style>
