<script setup lang="ts">
// 全局菜单搜索弹窗，交互参考 Vben Admin 的 global-search（MIT）
import { useEventListener } from '@vueuse/core';
import { Modal } from 'antdv-next';
import { computed, nextTick, ref, shallowRef, useTemplateRef, watch } from 'vue';
import type { MenuNode } from '@/platform/routing';
import { shellTexts } from '@/platform/texts/shell';
import KIcon from '@/platform/ui/KIcon.vue';
import {
  loadPinyinMatcher,
  matchInOrder,
  searchMenu,
  toSearchEntries,
  type Matcher,
  type SearchHit,
} from './menu-search';
import { addToHistory, loadSearchHistory, saveSearchHistory } from './search-history';

const { nodes, userId } = defineProps<{ nodes: readonly MenuNode[]; userId: string }>();
const emit = defineEmits<{ select: [routeName: string] }>();
const open = defineModel<boolean>('open', { required: true });
const t = shellTexts.search;
const input = useTemplateRef<HTMLInputElement>('input');
const list = useTemplateRef<HTMLElement>('list');

const keyword = ref('');
const activeIndex = ref(0);
// 键盘方向键导航滚动列表时锁定鼠标悬停选中，避免指针被动触发高亮切换
const keyboardMoving = ref(false);
const matcher = shallowRef<Matcher>(matchInOrder);
let pinyinRequested = false;

const history = ref<string[]>(loadSearchHistory(localStorage, userId));
watch(
  () => userId,
  (id) => {
    history.value = loadSearchHistory(localStorage, id);
  },
);

const entries = computed(() => toSearchEntries(nodes));
// 过滤掉无访问权限或已下线的最近访问记录
const recent = computed<SearchHit[]>(() =>
  history.value.flatMap((name) => {
    const entry = entries.value.find((item) => item.routeName === name);
    return entry ? [{ entry, indices: [] }] : [];
  }),
);
const searching = computed(() => keyword.value.trim() !== '');
const hits = computed(() => (searching.value ? searchMenu(entries.value, keyword.value, matcher.value) : recent.value));

watch(hits, () => {
  activeIndex.value = 0;
});

watch(open, async (isOpen) => {
  if (!isOpen) {
    keyword.value = '';
    return;
  }
  if (!pinyinRequested) {
    pinyinRequested = true;
    // 拼音库懒加载失败时降级为纯文本子序列匹配
    loadPinyinMatcher().then(
      (loaded) => {
        matcher.value = loaded;
      },
      () => undefined,
    );
  }
  await nextTick();
  input.value?.focus();
});

// 监听 Ctrl+K / ⌘+K，并阻止浏览器默认行为
useEventListener(window, 'keydown', (event: KeyboardEvent) => {
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
    event.preventDefault();
    open.value = !open.value;
  }
});

useEventListener(window, 'mousemove', () => {
  keyboardMoving.value = false;
});

function move(step: number): void {
  const count = hits.value.length;
  if (count === 0) {
    return;
  }
  keyboardMoving.value = true;
  activeIndex.value = (activeIndex.value + step + count) % count;
  list.value?.querySelector(`[data-index="${String(activeIndex.value)}"]`)?.scrollIntoView({ block: 'nearest' });
}

function choose(index: number): void {
  const hit = hits.value[index];
  if (hit === undefined) {
    return;
  }
  history.value = addToHistory(history.value, hit.entry.routeName);
  saveSearchHistory(localStorage, userId, history.value);
  open.value = false;
  emit('select', hit.entry.routeName);
}

function remove(routeName: string): void {
  history.value = history.value.filter((name) => name !== routeName);
  saveSearchHistory(localStorage, userId, history.value);
}

function onKeydown(event: KeyboardEvent): void {
  // 输入法组合输入阶段（IME）忽略回车与方向键，交由输入法处理
  if (event.isComposing) {
    return;
  }
  if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
    event.preventDefault();
    move(event.key === 'ArrowDown' ? 1 : -1);
  } else if (event.key === 'Enter') {
    event.preventDefault();
    choose(activeIndex.value);
  }
}

function hover(index: number): void {
  if (!keyboardMoving.value) {
    activeIndex.value = index;
  }
}

function segments(title: string, indices: readonly number[]): { text: string; hit: boolean }[] {
  const marked = new Set(indices);
  const parts: { text: string; hit: boolean }[] = [];
  title.split('').forEach((char, index) => {
    const hit = marked.has(index);
    const last = parts.at(-1);
    if (last?.hit === hit) {
      last.text += char;
    } else {
      parts.push({ text: char, hit });
    }
  });
  return parts;
}
</script>

<template>
  <Modal
    v-model:open="open"
    :width="600"
    :classes="{ container: 'search-container', header: 'search-header', body: 'search-body', footer: 'search-footer' }"
    root-class="kmic-search"
  >
    <template #title>
      <label class="search-input">
        <KIcon name="search" :size="16" />
        <input
          ref="input"
          v-model="keyword"
          :placeholder="t.placeholder"
          :aria-label="t.button"
          role="combobox"
          aria-expanded="true"
          aria-controls="kmic-search-list"
          :aria-activedescendant="hits.length > 0 ? `kmic-search-item-${String(activeIndex)}` : undefined"
          @keydown="onKeydown"
        />
      </label>
    </template>

    <div class="search-panel">
      <div v-if="searching && hits.length === 0" class="search-empty">
        <KIcon name="search-x" :size="48" />
        <p>
          {{ t.noResults }}
          <span class="search-keyword">“{{ keyword.trim() }}”</span>
        </p>
      </div>
      <p v-else-if="!searching && hits.length === 0" class="search-empty search-empty-recent">{{ t.noRecent }}</p>

      <ul v-show="hits.length > 0" id="kmic-search-list" ref="list" class="search-list" role="listbox">
        <li v-if="!searching" class="search-caption" role="presentation">{{ t.recent }}</li>
        <li
          v-for="(hit, index) in hits"
          :id="`kmic-search-item-${String(index)}`"
          :key="hit.entry.routeName"
          :data-index="index"
          class="search-item"
          :class="{ 'is-active': index === activeIndex }"
          role="option"
          :aria-selected="index === activeIndex"
          @click="choose(index)"
          @mouseenter="hover(index)"
        >
          <KIcon :name="hit.entry.icon ?? 'file'" :size="20" />
          <span class="search-title">
            <template v-for="(part, i) in segments(hit.entry.title, hit.indices)" :key="i">
              <mark v-if="part.hit">{{ part.text }}</mark>
              <template v-else>{{ part.text }}</template>
            </template>
          </span>
          <span v-if="hit.entry.folders.length > 0" class="search-folders">{{ hit.entry.folders.join(' / ') }}</span>
          <button
            v-if="!searching"
            class="search-remove"
            :title="t.remove"
            :aria-label="t.remove"
            @click.stop="remove(hit.entry.routeName)"
          >
            <KIcon name="close" :size="16" />
          </button>
        </li>
      </ul>
    </div>

    <template #footer>
      <div class="search-keys">
        <span><KIcon name="corner-down-left" :size="12" />{{ t.open }}</span>
        <span><KIcon name="arrow-up" :size="12" /><KIcon name="arrow-down" :size="12" />{{ t.move }}</span>
        <span><kbd>Esc</kbd>{{ t.close }}</span>
      </div>
    </template>
  </Modal>
</template>

<style lang="scss">
// 弹窗挂载于 body，需使用全局非 scoped 样式
.kmic-search {
  .search-container {
    padding: 0;
  }

  .search-header {
    padding: 8px 48px 8px 16px;
    margin: 0;
    border-bottom: 1px solid var(--kmic-color-border);
  }

  .search-body {
    padding: 16px 8px 4px;
  }

  .search-footer {
    padding: 10px 16px;
    margin: 0;
    border-top: 1px solid var(--kmic-color-border);
  }

  .search-input {
    display: flex;
    gap: 8px;
    align-items: center;
    color: var(--kmic-color-muted-foreground);

    input {
      flex: 1;
      min-width: 0;
      padding: 8px 0;
      font: inherit;
      font-weight: normal;
      color: var(--kmic-color-foreground);
      background: none;
      border: 0;
      outline: none;

      &::placeholder {
        color: var(--kmic-color-muted-foreground);
      }
    }
  }

  .search-panel {
    max-height: 450px;
    padding: 0 8px;
    overflow-y: auto;
  }

  .search-empty {
    color: var(--kmic-color-muted-foreground);
    text-align: center;

    svg {
      margin-top: 16px;
    }

    p {
      margin: 24px 0 40px;
      font-size: 12px;
    }
  }

  .search-empty-recent {
    margin: 40px 0;
    font-size: 12px;
  }

  .search-keyword {
    font-size: 14px;
    font-weight: 500;
    color: var(--kmic-color-foreground);
  }

  .search-list {
    padding: 0;
    margin: 0;
    list-style: none;
  }

  .search-caption {
    margin-bottom: 8px;
    font-size: 12px;
    color: var(--kmic-color-muted-foreground);
  }

  .search-item {
    display: flex;
    gap: 8px;
    align-items: center;
    padding: 16px;
    margin-bottom: 12px;
    cursor: pointer;
    background: var(--kmic-color-accent);
    border-radius: var(--kmic-radius);

    mark {
      font-weight: 600;
      color: var(--kmic-color-primary);
      background: none;
    }

    &.is-active {
      color: var(--kmic-color-primary-foreground);
      background: var(--kmic-color-primary);

      mark,
      .search-folders {
        color: inherit;
      }
    }
  }

  .search-title {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .search-folders {
    flex: none;
    font-size: 12px;
    color: var(--kmic-color-muted-foreground);
  }

  .search-remove {
    display: flex;
    padding: 4px;
    color: inherit;
    cursor: pointer;
    background: none;
    border: 0;
    border-radius: 50%;
    transition: transform var(--kmic-motion-fast);

    &:hover {
      transform: scale(1.1);
    }
  }

  .search-keys {
    display: flex;
    gap: 8px;
    font-size: 12px;
    color: var(--kmic-color-muted-foreground);

    span {
      display: flex;
      gap: 2px;
      align-items: center;
    }

    kbd {
      margin-right: 2px;
      font: inherit;
    }
  }
}
</style>
