// 违规示例：页面直接调用 useQuery，绕过了 page.useQuery 的失活暂停控制
import { useQuery } from '@tanstack/vue-query';

export const raw = useQuery;
