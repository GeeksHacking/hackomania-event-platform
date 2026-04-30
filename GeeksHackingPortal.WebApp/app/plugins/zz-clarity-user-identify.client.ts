import { geeksHackingPortalApiEndpointsAuthWhoAmIEndpointQueryOptions } from '@geekshacking/portal-sdk/hooks'
import { useQueryClient } from '@tanstack/vue-query'

declare global {
  interface Window {
    clarity?: (...args: [string, ...unknown[]]) => void
  }
}

export default defineNuxtPlugin((nuxtApp) => {
  const queryClient = useQueryClient()

  nuxtApp.hook('app:mounted', async () => {
    // Skip identification when Clarity is unavailable (blocked script, CSP, etc.)
    if (typeof window.clarity !== 'function') {
      return
    }

    try {
      const user = await queryClient.fetchQuery(geeksHackingPortalApiEndpointsAuthWhoAmIEndpointQueryOptions())
      const userId = user?.id

      if (userId) {
        window.clarity('identify', userId)
      }
    }
    catch {
      // User is likely not authenticated; keep Clarity in anonymous mode.
    }
  })
})
