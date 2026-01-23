<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'

definePageMeta({
  // Explicitly mark as public route
  auth: false,
})

const route = useRoute()
const config = useRuntimeConfig()
const hackathonId = computed(() => (route.params.hackathonId as string | undefined) ?? null)

// Track if we should show the form
const showForm = ref(false)

// Check if user is authenticated
const { data: user, isLoading, isError } = useQuery({
  ...authQueries.whoAmI,
  retry: false,
  staleTime: 0,
  gcTime: 0,
})

// Handle auth state changes
watchEffect(() => {
  if (!isLoading.value) {
    if (user.value && !isError.value) {
      showForm.value = true
    }
    else if (hackathonId.value) {
      navigateTo(`${config.public.api}/auth/login?redirect_uri=${encodeURIComponent(route.fullPath)}`, { external: true })
    }
  }
})
</script>

<template>
  <!-- Show loading while checking auth -->
  <div
    v-if="isLoading || !showForm"
    class="bg-white min-h-screen flex flex-col items-center justify-center gap-4 px-4"
  >
    <p class="text-sm font-medium text-gray-600 animate-pulse">
      Checking your session...
    </p>
    <UIcon name="i-lucide-loader-circle" class="w-8 h-8 animate-spin text-primary" />
  </div>

  <!-- Show form if authenticated -->
  <RegistrationFormPage
    v-else
    :hackathon-id="hackathonId"
  />
</template>
