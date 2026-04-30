import type { RequestConfig } from '@geekshacking/portal-sdk/client'
import { setConfig } from '@geekshacking/portal-sdk/client'

type PortalRequestConfig = RequestConfig & {
  withCredentials?: boolean
}

export default defineNuxtPlugin(() => {
  const runtimeConfig = useRuntimeConfig()

  setConfig({
    baseURL: runtimeConfig.public.api,
    withCredentials: true,
  } satisfies PortalRequestConfig)
})
