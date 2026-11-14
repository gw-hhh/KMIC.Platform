// 从后端导出的 OpenAPI 文档生成请求层用的类型：src/generated/api.d.ts，禁止手改
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
const target = resolve(root, 'src/generated/api.d.ts');

const header = '// 由 scripts/generate-api.mjs 从 OpenAPI 文档生成，禁止手改\n\n';
const generated = header + astToString(await openapiTS(pathToFileURL(source)));

if (check) {
  const current = await readFile(target, 'utf8').catch(() => '');
  if (current !== generated) {
    console.error('src/generated/api.d.ts 和 OpenAPI 文档不一致，请运行 pnpm generate:api 并提交');
    process.exit(1);
  }
  console.log('src/generated/api.d.ts 与 OpenAPI 文档一致');
} else {
  await writeFile(target, generated, 'utf8');
  console.log(`已生成 ${target}`);
}
