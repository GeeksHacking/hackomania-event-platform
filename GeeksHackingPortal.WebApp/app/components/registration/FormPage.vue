<script setup lang="ts">
import {
  geeksHackingPortalApiEndpointsParticipantsHackathonRegistrationQuestionsListEndpoint,
  geeksHackingPortalApiEndpointsParticipantsHackathonRegistrationQuestionsListEndpointQueryOptions,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonRegistrationQuestionsInitializeEndpoint,
  useGeeksHackingPortalApiEndpointsParticipantsHackathonJoinEndpoint,
  useGeeksHackingPortalApiEndpointsParticipantsHackathonStatusEndpoint,
} from '@geekshacking/portal-sdk/hooks'
import { useQuery, useQueryClient } from '@tanstack/vue-query'

const props = defineProps<{
  hackathonId?: string | null
}>()

useHead({
  titleTemplate: title => (title ? `${title} - HackOMania` : 'HackOMania'),
})

const hackathonId = computed(() => props.hackathonId ?? null)

const setupComplete = ref(false)
const setupError = ref<string | null>(null)
const lastSetupHackathonId = ref<string | null>(null)
const canJoinFromHere = ref(false)

// Initialize mutations at component level (must be in setup)
const initQuestionMutation = useGeeksHackingPortalApiEndpointsOrganizersHackathonRegistrationQuestionsInitializeEndpoint()
const joinMutation = useGeeksHackingPortalApiEndpointsParticipantsHackathonJoinEndpoint()
const queryClient = useQueryClient()

// Ensure user is a participant before attempting to load registration data.
const { data: statusData, isLoading: isLoadingStatus, error: statusError } = useGeeksHackingPortalApiEndpointsParticipantsHackathonStatusEndpoint(
  computed(() => hackathonId.value ?? ''),
  { query: { enabled: computed(() => !!hackathonId.value) } },
)

watch(
  () => ({ id: hackathonId.value, status: statusData.value }),
  async ({ id, status }) => {
    if (!id || status === undefined)
      return

    // If not a participant, do not auto-join. This prevents withdrawn participants
    // from being silently re-joined by registration initialization.
    if (!status.isParticipant) {
      setupComplete.value = false
      lastSetupHackathonId.value = null
      canJoinFromHere.value = true
      setupError.value = 'You are not currently participating in this hackathon. Please join to continue.'
      return
    }

    canJoinFromHere.value = false

    if (lastSetupHackathonId.value === id && setupComplete.value)
      return

    setupError.value = null
    setupComplete.value = false

    try {
      const questionsResponse = await geeksHackingPortalApiEndpointsParticipantsHackathonRegistrationQuestionsListEndpoint(id)
      const categories = questionsResponse?.categories ?? []
      const hasQuestions = categories.some(cat => cat.questions && cat.questions.length > 0)

      if (!hasQuestions)
        await initQuestionMutation.mutateAsync({ hackathonId: id })

      lastSetupHackathonId.value = id
      setupComplete.value = true
      await queryClient.invalidateQueries({ queryKey: [{ url: '/participants/hackathons/:hackathonId/status', params: { hackathonId: id } }] })
    }
    catch (error) {
      console.error('[FORM] Registration setup failed:', error)
      setupError.value = 'Failed to initialize registration. Please try again.'
    }
  },
  { immediate: true },
)

// Fetch registration questions (only after setup is complete)
const { data: questions, isLoading, error } = useQuery(computed(() => ({
  ...geeksHackingPortalApiEndpointsParticipantsHackathonRegistrationQuestionsListEndpointQueryOptions(hackathonId.value ?? ''),
  enabled: setupComplete.value && !!hackathonId.value,
})))

async function joinHackathonFromRegistration() {
  if (!hackathonId.value)
    return

  try {
    await joinMutation.mutateAsync({ hackathonId: hackathonId.value })
    await queryClient.invalidateQueries({ queryKey: [{ url: '/participants/hackathons/:hackathonId/status', params: { hackathonId: hackathonId.value } }] })
    setupError.value = null
  }
  catch (joinError) {
    console.error('[FORM] Failed to join hackathon:', joinError)
    setupError.value = 'Unable to join right now. Please try again from the participant dashboard.'
  }
}
</script>

<template>
  <div class="bg-white dark:bg-gray-900 min-h-screen font-raleway">
    <RegistrationFormHeader :hackathon-id="hackathonId" />

    <div v-if="!hackathonId" class="text-center py-8 space-y-2">
      <p class="text-gray-900 dark:text-gray-100">
        Unable to load registration. Please return to the website and try again.
      </p>
      <NuxtLink to="https://hackomania.geekshacking.com/" external class="text-blue-600 dark:text-blue-400 underline">
        Return to HackOMania
      </NuxtLink>
    </div>

    <div
      v-else-if="isLoadingStatus || isLoading"
      class="text-center py-8"
    >
      <p class="text-gray-900 dark:text-gray-100">
        Loading registration questions...
      </p>
    </div>

    <div
      v-else-if="statusError"
      class="text-center py-8"
    >
      <p class="text-red-600 dark:text-red-400">
        Unable to load your participation status. Please return to the dashboard and try again.
      </p>
    </div>

    <!-- Setup Error State -->
    <div
      v-else-if="setupError"
      class="text-center py-8"
    >
      <p class="text-red-600 dark:text-red-400">
        {{ setupError }}
      </p>
      <div
        v-if="canJoinFromHere"
        class="mt-4"
      >
        <UButton
          color="neutral"
          variant="solid"
          :loading="joinMutation.isPending.value"
          @click="joinHackathonFromRegistration"
        >
          Join hackathon
        </UButton>
      </div>
    </div>

    <div
      v-else-if="!setupComplete"
      class="text-center py-8"
    >
      <p class="text-gray-900 dark:text-gray-100">
        Preparing your registration form...
      </p>
    </div>

    <!-- Error State -->
    <div
      v-else-if="error"
      class="text-center py-8"
    >
      <p class="text-red-600 dark:text-red-400">
        Error loading questions: {{ error.message }}
      </p>
    </div>

    <!-- Render Form with Questions -->
    <div v-else-if="questions">
      <RegistrationDynamicForm
        :questions="questions"
        :hackathon-id="hackathonId!"
        @section-change="onSectionChange"
      />
    </div>
  </div>
</template>
