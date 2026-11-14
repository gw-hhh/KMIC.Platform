/// <reference types="vite/client" />

// typescript-eslint 的类型服务不认 .vue 文件，没有这个声明时 .vue 的导入会被当成错误类型；vue-tsc 仍按真实文件检查
declare module '*.vue' {
  import type { DefineComponent } from 'vue';
  const component: DefineComponent;
  export default component;
}
