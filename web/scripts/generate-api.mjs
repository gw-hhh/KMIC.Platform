// 从后端导出的 OpenAPI 文档生成 src/generated/ 下的文件，禁止手改：
//   api.d.ts            请求层用的路径、参数、响应类型
//   permissions.ts      权限点（x-kmic-permissions）
//   error-codes.ts      错误码（x-kmic-error-codes）
//   dictionary-types.ts 字典类型（x-kmic-dictionary-types）
//   enum-labels.ts      智能枚举的显示名（x-kmic-enums）
//   upload-profiles.ts  上传档案（x-kmic-upload-profiles）
//   operations.ts       要带幂等键的写操作（x-idempotent）
//
// 文档由 Kmic.Api.Tests 里的 OpenApiDocumentTests 导出到 artifacts/openapi/openapi.v1.json，
// 所以要先跑一次后端测试。也可以传文件路径：pnpm generate:api -- <路径>
// 加 --check 只比对不写入，生成的内容和已提交的不一样时失败，CI 用它

import { readFile, writeFile } from 'node:fs/promises';
import { resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import openapiTS, { astToString } from 'openapi-typescript';

const root = fileURLToPath(new URL('..', import.meta.url));
const args = process.argv.slice(2).filter((a) => a !== '--');
const check = args.includes('--check');
const source = resolve(root, args.find((a) => !a.startsWith('--')) ?? '../artifacts/openapi/openapi.v1.json');
const header = '// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改\n\n';

const document = JSON.parse(await readFile(source, 'utf8'));

const files = {
  'api.d.ts': astToString(await openapiTS(pathToFileURL(source))),
  'permissions.ts': codesFile('PermissionCode', 'Permissions', document['x-kmic-permissions'], '权限点'),
  'error-codes.ts': codesFile('ErrorCode', 'ErrorCodes', document['x-kmic-error-codes'], '错误码'),
  'dictionary-types.ts': dictionaryTypesFile(document['x-kmic-dictionary-types']),
  'enum-labels.ts': enumLabelsFile(document['x-kmic-enums']),
  'upload-profiles.ts': uploadProfilesFile(document['x-kmic-upload-profiles']),
  'operations.ts': operationsFile(document.paths),
};

let stale = false;
for (const [name, content] of Object.entries(files)) {
  const target = resolve(root, 'src/generated', name);
  const generated = header + content;
  if (check) {
    const current = await readFile(target, 'utf8').catch(() => '');
    if (current !== generated) {
      console.error(`src/generated/${name} 和 OpenAPI 文档不一致`);
      stale = true;
    }
  } else {
    await writeFile(target, generated, 'utf8');
  }
}

if (stale) {
  console.error('请运行 pnpm generate:api 并提交');
  process.exit(1);
}
console.log(check ? 'src/generated/ 与 OpenAPI 文档一致' : '已生成 src/generated/');

function literal(value) {
  return JSON.stringify(value);
}

function union(values) {
  return values.length === 0 ? 'never' : values.map(literal).join(' | ');
}

/** `A.B.C` 拆成嵌套对象，叶子是完整编码：Permissions.Sample.Specimen.View === 'Sample.Specimen.View' */
function codesFile(typeName, constName, items, label) {
  const codes = items.map((item) => item.code).sort();
  const tree = {};
  for (const code of codes) {
    const parts = code.split('.');
    let node = tree;
    parts.forEach((part, index) => {
      if (!/^[A-Za-z][A-Za-z0-9]*$/.test(part)) {
        throw new Error(`${label} ${code} 的“${part}”不能作为属性名`);
      }
      const last = index === parts.length - 1;
      const existing = node[part];
      if (last ? existing !== undefined : typeof existing === 'string') {
        throw new Error(`${label} ${code} 和另一个编码的前缀冲突`);
      }
      node[part] = last ? code : (existing ?? {});
      node = node[part];
    });
  }

  const render = (node, indent) =>
    `{\n${Object.entries(node)
      .map(
        ([key, value]) =>
          `${indent}  ${key}: ${typeof value === 'string' ? literal(value) : render(value, `${indent}  `)},`,
      )
      .join('\n')}\n${indent}}`;

  return `export type ${typeName} = ${union(codes)};\n\nexport const ${constName} = ${render(tree, '')} as const;\n`;
}

function dictionaryTypesFile(items) {
  const sorted = [...items].sort((a, b) => a.code.localeCompare(b.code));
  const entries = sorted
    .map(
      (item) => `  ${literal(item.code)}: { name: ${literal(item.name)}, hierarchical: ${String(item.hierarchical)} },`,
    )
    .join('\n');
  return (
    `export type DictionaryType = ${union(sorted.map((item) => item.code))};\n\n` +
    `export const dictionaryTypes: Readonly<Record<DictionaryType, { readonly name: string; readonly hierarchical: boolean }>> = {\n${entries}\n};\n`
  );
}

// 选项按后端声明的顺序，前端不另排
function enumLabelsFile(enums) {
  const names = Object.keys(enums).sort();
  const entries = names
    .map((name) => {
      const members = enums[name].map((m) => `    { code: ${literal(m.code)}, name: ${literal(m.name)} },`).join('\n');
      return `  ${name}: [\n${members}\n  ],`;
    })
    .join('\n');
  return `export const enumLabels = {\n${entries}\n} as const;\n\nexport type EnumName = keyof typeof enumLabels;\n`;
}

function uploadProfilesFile(profiles) {
  const names = Object.keys(profiles).sort();
  const entries = names.map((name) => `  ${literal(name)}: ${JSON.stringify(profiles[name])},`).join('\n');
  return (
    `export const UploadProfiles = {${entries === '' ? '' : `\n${entries}\n`}} as const;\n\n` +
    `export type UploadProfileName = keyof typeof UploadProfiles;\n`
  );
}

function operationsFile(paths) {
  const idempotent = Object.entries(paths)
    .flatMap(([path, operations]) =>
      Object.entries(operations)
        .filter(([, operation]) => operation?.['x-idempotent'] === true)
        .map(([method]) => `${method.toUpperCase()} ${path}`),
    )
    .sort();
  return (
    `/** 要带幂等键的写操作，键为“方法 路径”（与 api.d.ts 的路径写法相同） */\n` +
    `export const idempotentOperations: ReadonlySet<string> = new Set([\n${idempotent.map((op) => `  ${literal(op)},`).join('\n')}\n]);\n`
  );
}
