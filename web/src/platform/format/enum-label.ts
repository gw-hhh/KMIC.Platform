import { enumLabels, type EnumName } from '@/generated/enum-labels';

/** 获取智能枚举编码对应的显示名称；若前端版本落后未包含该编码，则回退为原编码 */
export function enumLabel(name: EnumName, code: string): string {
  const members: readonly { code: string; name: string }[] = enumLabels[name];
  return members.find((member) => member.code === code)?.name ?? code;
}
