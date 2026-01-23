<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'

const route = useRoute()
const hackathonId = route.params.hackathonId as string

// Middleware handles authentication, just check participant status
const { data: status, isLoading: statusLoading } = useQuery(hackathonQueries.status(hackathonId))

// Compute if we should show content (only when verified as participant)
const isParticipant = computed(() => !statusLoading.value && status.value?.isParticipant === true)
const shouldShowLoading = computed(() => statusLoading.value || (status.value && !status.value.isParticipant))

watch(
  [() => status.value, statusLoading],
  ([statusData, statusIsLoading]) => {
    if (statusIsLoading) return
    if (!statusData?.isParticipant) {
      navigateTo(`/${hackathonId}/registration`, { replace: true })
    }
  },
  { immediate: true },
)
</script>

<template>
  <!-- Loading state while checking participant status -->
  <div v-if="shouldShowLoading" class="min-h-screen flex flex-col items-center justify-center gap-4">
    <p class="text-sm font-medium text-gray-600 animate-pulse">
      Authenticating...
    </p>
    <UIcon name="i-lucide-loader-circle" class="w-8 h-8 animate-spin text-primary" />
  </div>

  <!-- Main content - only show when verified as participant -->
  <div v-else-if="isParticipant">
    <AppNavBar />
    <div id="home" class="scroll-mt-12 lg:scroll-mt-18">
      <AppHeroSection />
    </div>
    <div id="challenges" class="scroll-mt-12 lg:scroll-mt-18">
      <PortalChallengesSection />
    </div>
    <div id="team" class="scroll-mt-12 lg:scroll-mt-18">
      <PortalTeamSection />
    </div>
    <AppFooter />
  </div>
</template>
