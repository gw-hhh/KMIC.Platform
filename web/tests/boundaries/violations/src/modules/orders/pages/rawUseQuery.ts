// 页面里直接用 useQuery，绕过了 page.useQuery 的失活暂停
import { useQuery } from '@tanstack/vue-query';

export const raw = useQuery;
