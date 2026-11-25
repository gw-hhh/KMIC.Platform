// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改

export type DictionaryType = "Sample.Category" | "Sample.Origin";

export const dictionaryTypes: Readonly<Record<DictionaryType, { readonly name: string; readonly hierarchical: boolean }>> = {
  "Sample.Category": { name: "示例类别", hierarchical: false },
  "Sample.Origin": { name: "产地", hierarchical: true },
};
