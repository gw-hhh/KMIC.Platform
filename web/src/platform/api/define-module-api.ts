import { queryOptions, type QueryKey } from '@tanstack/vue-query';
import type { FetchResponse, ParamsOption, RequestBodyOption } from 'openapi-fetch';
import type { paths } from '@/generated/api';
import { idempotentOperations } from '@/generated/operations';
import type { ApiClient } from './client';
import { apiOf } from './query-client';

type Method = 'get' | 'post' | 'put' | 'patch' | 'delete';
export type CommandMethod = 'POST' | 'PUT' | 'PATCH' | 'DELETE';

/** 有这个方法的路径 */
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

/** 没有必填参数时可以不传 */
export type ArgsOf<Input> = object extends Input ? [input?: Input] : [input: Input];

export type QueryInput<P extends PathOf<'get'>> = ParamsOption<Operation<'get', P>>;
// GET 总有响应体；openapi-fetch 的成功类型里带着 undefined，去掉
export type QueryData<P extends PathOf<'get'>> = Exclude<DataOf<Operation<'get', P>>, undefined>;

// 幂等键由 useCommand 按提交意图生成，调用方不传
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
  /** OpenAPI 里标了 x-idempotent，提交时要带同一个幂等键直到有确定结果 */
  readonly idempotent: boolean;
  send(api: ApiClient, input: Input, options?: { idempotencyKey?: string }): Promise<Data>;
}

// 生成的客户端按路径推断类型，泛型路径下推不出来；类型在 defineModuleApi 的签名上保证
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
    /** 这个查询的所有参数组合，失效时用 */
    all,
    /** 一组具体参数 */
    key,
    /** 交给 useQuery 的选项 */
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
  /** 只有 GET 是查询 */
  query: <P extends PathOf<'get'>>(path: P) => ReturnType<typeof createQuery<P>>;
  /** 其余方法都是命令，经 useCommand 执行 */
  command: <M extends CommandMethod, P extends PathOf<Lowercase<M>>>(
    method: M,
    path: P,
  ) => CommandDef<CommandInput<M, P>, CommandData<M, P>>;
}

/**
 * 一个模块的查询与命令，路径、参数、响应类型都从生成的 paths 推断
 */
export function defineModuleApi<T>(module: string, build: (builder: ModuleApiBuilder) => T): T {
  return build({
    query: (path) => createQuery(module, path),
    command: (method, path) => createCommand(module, method, path),
  });
}
