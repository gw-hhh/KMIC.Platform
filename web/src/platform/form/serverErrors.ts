import type { AnyFieldApi, AnyFormApi } from '@tanstack/vue-form';

export interface ServerFieldError {
  path: string;
  code: string;
  message: string;
}

/**
 * 把 ProblemDetails 的 errors[] 按路径回填到字段；找不到对应字段的返回给调用方汇总显示。
 */
export function applyServerErrors(form: AnyFormApi, errors: readonly ServerFieldError[]): ServerFieldError[] {
  const unmatched: ServerFieldError[] = [];
  for (const error of errors) {
    if (!(error.path in form.fieldInfo)) {
      unmatched.push(error);
      continue;
    }
    form.setFieldMeta(error.path, (meta) => ({
      ...meta,
      errorMap: { ...meta.errorMap, onServer: error.message },
    }));
  }
  return unmatched;
}

/** 用户改了字段之后，这个字段上的服务端错误就不再成立。 */
export function clearServerError({ fieldApi }: { fieldApi: AnyFieldApi }): void {
  if (fieldApi.state.meta.errorMap.onServer === undefined) {
    return;
  }
  fieldApi.setMeta((meta) => ({ ...meta, errorMap: { ...meta.errorMap, onServer: undefined } }));
}
