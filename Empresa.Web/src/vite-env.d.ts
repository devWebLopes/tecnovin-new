/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_BI_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
