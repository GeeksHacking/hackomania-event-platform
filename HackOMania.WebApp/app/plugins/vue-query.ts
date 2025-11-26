import { DefaultApiError } from '@microsoft/kiota-abstractions'
import type {
  DehydratedState,
  VueQueryPluginOptions,
} from '@tanstack/vue-query'
import {
  VueQueryPlugin,
  QueryClient,
  hydrate,
  dehydrate,
  QueryCache,
  MutationCache,
} from '@tanstack/vue-query'

export default defineNuxtPlugin((nuxt) => {
  const vueQueryState = useState<DehydratedState | null>('vue-query')
  const config = useRuntimeConfig()

  const queryClient = new QueryClient({
    defaultOptions: { queries: { staleTime: 5000 } },
    queryCache: new QueryCache({
      async onError(error) {
        if (error instanceof DefaultApiError) {
          if (error?.responseStatusCode === 401) {
            await navigateTo(`${config.public.api}/auth/login`, { external: true })
          }
        }
      },
    }),
    mutationCache: new MutationCache({
      async onError(error) {
        if (error instanceof DefaultApiError) {
          if (error?.responseStatusCode === 401) {
            await navigateTo(`${config.public.api}/auth/login`, { external: true })
          }
        }
      },
    }),
  })
  const options: VueQueryPluginOptions = { queryClient }

  nuxt.vueApp.use(VueQueryPlugin, options)

  if (import.meta.server) {
    nuxt.hooks.hook('app:rendered', () => {
      vueQueryState.value = dehydrate(queryClient)
    })
  }

  if (import.meta.client) {
    hydrate(queryClient, vueQueryState.value)
  }
})
