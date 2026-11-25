import { enumLabels, type EnumName } from '@/generated/enum-labels';

/** 智能枚举编码的显示名；前端版本落后、遇到没生成的编码时原样显示 */
export function enumLabel(name: EnumName, code: string): string {
  const members: readonly { code: string; name: string }[] = enumLabels[name];
  return members.find((member) => member.code === code)?.name ?? code;
}
