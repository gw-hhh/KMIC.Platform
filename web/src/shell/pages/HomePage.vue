<script setup lang="ts">
import { Card } from 'antdv-next';
import { useCurrentUserStore } from '@/platform/session/current-user';
import { shellTexts } from '@/platform/texts/shell';

const currentUser = useCurrentUserStore();
</script>

<template>
  <Card :title="shellTexts.pages.homeWelcome">
    <p>{{ currentUser.me?.displayName }}，{{ currentUser.me?.currentRole.name }}</p>
  </Card>
</template>
