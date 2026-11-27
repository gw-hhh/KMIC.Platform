import { queryOptions, type QueryKey } from '@tanstack/vue-query';
import type { FetchResponse, ParamsOption, RequestBodyOption } from 'openapi-fetch';
import type { paths } from '@/generated/api';
import { idempotentOperations } from '@/generated/operations';
import type { ApiClient } from './client';
import { apiOf } from './query-client';

type Method = 'get' | 'post' | 'put' | 'patch' | 'delete';
export type CommandMethod = 'POST' | 'PUT' | 'PATCH' | 'DELETE';

/** 支持指定 HTTP 方法的接口路径联合类型 */
export type PathOf<M extends Method> = {
  [P in keyof paths]: [NonNullable<paths[P][M]>] extends [never] ? never : P;
}[keyof paths];

type Operation<M extends Method, P extends keyof paths> = NonNullable<paths[P][M]>;

type DataOf<Op> =
  Op extends Record<string | number, unknown>
    ? Extract<FetchResponse<Op, unknown, 'application/json'>, { error?: never }>['data'] extends infer Data
      ? [Data] extends [never]
        ? undefined
        : Data
      : never
    : never;

/** 无必填参数时允许省略入参 */
export type ArgsOf<Input> = object extends Input ? [input?: Input] : [input: Input];

export type QueryInput<P extends PathOf<'get'>> = ParamsOption<Operation<'get', P>>;
// GET 请求必有响应体，剔除 openapi-fetch 成功类型中附带的 undefined
export type QueryData<P extends PathOf<'get'>> = Exclude<DataOf<Operation<'get', P>>, undefined>;

// 幂等键由 useCommand 按提交意图统一生成，禁止调用方手动传入
type WithoutIdempotencyKey<Op> = Op extends { parameters: infer Params }
  ? Omit<Op, 'parameters'> & { parameters: Omit<Params, 'header'> & { header?: never } }
  : Op;

export type CommandInput<M extends CommandMethod, P extends PathOf<Lowercase<M>>> = ParamsOption<
  WithoutIdempotencyKey<Operation<Lowercase<M>, P>>
> &
  RequestBodyOption<Operation<Lowercase<M>, P>>;
export type CommandData<M extends CommandMethod, P extends PathOf<Lowercase<M>>> = DataOf<Operation<Lowercase<M>, P>>;

export interface CommandDef<Input, Data> {
  readonly module: string;
  readonly method: CommandMethod;
  readonly path: string;
  /** OpenAPI 中标记为 x-idempotent，重试或确认时需复用同一幂等键直至获得明确结果 */
  readonly idempotent: boolean;
  send(api: ApiClient, input: Input, options?: { idempotencyKey?: string }): Promise<Data>;
}

// 生成的客户端按具体路径字面量推断类型，泛型路径下无法推导，类型安全由 defineModuleApi 签名约束
interface LooseClient {
  request(method: string, url: string, init: Record<string, unknown>): Promise<{ data?: unknown }>;
}

function loose(api: ApiClient): LooseClient {
  return api as unknown as LooseClient;
}

function createQuery<P extends PathOf<'get'>>(module: string, path: P) {
  const all: QueryKey = ['kmic', module, path];
  const key = (...[input]: ArgsOf<QueryInput<P>>): QueryKey => [
    ...all,
    (input as { params?: unknown } | undefined)?.params ?? {},
  ];

  return {
    path,
    /** 该查询下所有参数组合的前缀键，用于批量失效缓存 */
    all,
    key,
    options: (...args: ArgsOf<QueryInput<P>>) =>
      queryOptions({
        queryKey: key(...args),
        queryFn: async ({ client, signal }) => {
          const init = (args[0] ?? {}) as Record<string, unknown>;
          const { data } = await loose(apiOf(client)).request('GET', path, { ...init, signal });
          return data as QueryData<P>;
        },
      }),
  };
}

function createCommand<M extends CommandMethod, P extends PathOf<Lowercase<M>>>(
  module: string,
  method: M,
  path: P,
): CommandDef<CommandInput<M, P>, CommandData<M, P>> {
  return {
    module,
    method,
    path,
    idempotent: idempotentOperations.has(`${method} ${path}`),
    async send(api, input, options) {
      const key = options?.idempotencyKey;
      const init = {
        ...(input as Record<string, unknown>),
        ...(key === undefined ? {} : { headers: { 'Idempotency-Key': key } }),
      };
      const { data } = await loose(api).request(method, path, init);
      return data as CommandData<M, P>;
    },
  };
}

export interface ModuleApiBuilder {
  query: <P extends PathOf<'get'>>(path: P) => ReturnType<typeof createQuery<P>>;
  command: <M extends CommandMethod, P extends PathOf<Lowercase<M>>>(
    method: M,
    path: P,
  ) => CommandDef<CommandInput<M, P>, CommandData<M, P>>;
}

/** 定义模块的查询与命令集合，路径、入参及响应类型均从生成的 paths 自动推导 */
export function defineModuleApi<T>(module: string, build: (builder: ModuleApiBuilder) => T): T {
  return build({
    query: (path) => createQuery(module, path),
    command: (method, path) => createCommand(module, method, path),
  });
}
