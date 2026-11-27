<script setup lang="ts">
import { Button, Card, Input, Space } from 'antdv-next';
import { Permissions } from '@/generated/permissions';
import { useCan } from '@/platform/authz/use-can';
import { useNavigate } from '@/platform/routing';

const navigate = useNavigate();
const canCreate = useCan(Permissions.Sample.Specimen.Create);
const ids = ['SP-001', 'SP-002', 'SP-003'];
// 生成足够行以撑出滚动条，用于验证切换标签页后滚动位置与焦点的恢复
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
      <Input placeholder="滚动到底部输入内容，切换至其他标签页后再切回" aria-label="底部输入框" />
    </Card>
  </Space>
</template>

<style scoped lang="scss">
.demo-list {
  width: 100%;
}
</style>
