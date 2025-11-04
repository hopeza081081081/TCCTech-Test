/// <reference types="vite/client" />

declare module "*.vue" {
  import type { DefineComponent } from "vue";
  const component: DefineComponent<object, object, unknown>;
  export default component;
}
//vue3-barcode
//vue-barcode-generator
// declare module "vue-barcode-generator" {
//   import { DefineComponent } from "vue";
//   const VueBarcode: DefineComponent<object, object, unknown>;
//   export default VueBarcode;
// }
