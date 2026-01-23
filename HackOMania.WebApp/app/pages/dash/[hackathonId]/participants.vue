<script setup lang="ts">
import { computed, ref } from 'vue'
import { useQuery, useQueryClient } from '@tanstack/vue-query'
import { participantOrganizerQueries, useReviewParticipantMutation } from '~/composables/participants'

const props = defineProps<{
  hackathonId: string
  isOrganizer: boolean
}>()

const queryClient = useQueryClient()

const { data: participantsData, isLoading: isLoadingParticipants } = useQuery(
  computed(() => ({
    ...participantOrganizerQueries.list(props.hackathonId),
    enabled: !!props.hackathonId && props.isOrganizer,
  })),
)

const participants = computed(() => participantsData.value?.participants ?? [])

// Review mutation
const reviewMutation = useReviewParticipantMutation(props.hackathonId)

// Modal state
const isModalOpen = ref(false)
const reviewingParticipantId = ref<string | null>(null)
const reviewingParticipantName = ref<string | null>(null)
const reviewForm = ref({
  decision: 'approve',
  reason: '',
})

function openReviewModal(participantId: string, participantName: string | null, decision: string) {
  reviewingParticipantId.value = participantId
  reviewingParticipantName.value = participantName
  reviewForm.value = { decision, reason: '' }
  isModalOpen.value = true
}

async function handleReview() {
  if (!reviewingParticipantId.value) return
  await reviewMutation.mutateAsync({
    participantUserId: reviewingParticipantId.value,
    review: {
      decision: reviewForm.value.decision,
      reason: reviewForm.value.reason || null,
    },
  })
  await queryClient.invalidateQueries({ queryKey: ['hackathons', props.hackathonId, 'participants'] })
  isModalOpen.value = false
  reviewingParticipantId.value = null
  reviewingParticipantName.value = null
}

function getStatusColor(status: number | null | undefined): 'success' | 'error' | 'warning' {
  if (status === 1) return 'success'
  if (status === 2) return 'error'
  return 'warning'
}

function getStatusLabel(status: number | null | undefined): string {
  if (status === 1) return 'Approved'
  if (status === 2) return 'Rejected'
  return 'Pending'
}
</script>

<template>
  <div>
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-sm font-semibold">
            Participants
          </h3>
          <UBadge
            variant="subtle"
            size="sm"
          >
            {{ participants.length }} total
          </UBadge>
        </div>
      </template>

      <div
        v-if="isLoadingParticipants"
        class="text-muted text-sm"
      >
        Loading participants...
      </div>

      <div
        v-else-if="!participants.length"
        class="text-muted text-sm"
      >
        No participants yet.
      </div>

      <div
        v-else
        class="divide-y"
      >
        <div
          v-for="participant in participants"
          :key="participant.id ?? ''"
          class="py-2 flex items-center justify-between"
        >
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium">
              {{ participant.id }}
            </p>
            <p class="text-xs text-muted">
              Team: {{ participant.teamName ?? 'No team' }}
            </p>
          </div>
          <div class="flex items-center gap-2 ml-2">
            <UBadge
              :color="getStatusColor(participant.concludedStatus)"
              variant="subtle"
              size="xs"
            >
              {{ getStatusLabel(participant.concludedStatus) }}
            </UBadge>
            <UButton
              size="xs"
              variant="ghost"
              color="success"
              icon="i-lucide-check"
              @click="openReviewModal(participant.id ?? '', participant.teamName, 'approve')"
            />
            <UButton
              size="xs"
              variant="ghost"
              color="error"
              icon="i-lucide-x"
              @click="openReviewModal(participant.id ?? '', participant.teamName, 'reject')"
            />
          </div>
        </div>
      </div>
    </UCard>

    <UModal v-model:open="isModalOpen">
      <template #content>
        <UCard>
          <template #header>
            <div class="flex items-center justify-between">
              <h3 class="text-base font-semibold">
                {{ reviewForm.decision === 'approve' ? 'Approve' : 'Reject' }} Participant
              </h3>
              <UButton
                variant="ghost"
                icon="i-lucide-x"
                size="xs"
                @click="isModalOpen = false"
              />
            </div>
          </template>

          <form
            class="space-y-4"
            @submit.prevent="handleReview"
          >
            <p class="text-sm text-muted">
              {{ reviewForm.decision === 'approve' ? 'Approving' : 'Rejecting' }} participant: <strong>{{ reviewingParticipantId }}</strong>
            </p>

            <UFormField label="Reason (optional)">
              <UTextarea
                v-model="reviewForm.reason"
                :placeholder="reviewForm.decision === 'approve' ? 'Add a note for approval...' : 'Provide a reason for rejection...'"
                :rows="3"
              />
            </UFormField>

            <div class="flex justify-end gap-2">
              <UButton
                variant="ghost"
                @click="isModalOpen = false"
              >
                Cancel
              </UButton>
              <UButton
                type="submit"
                :color="reviewForm.decision === 'approve' ? 'success' : 'error'"
                :loading="reviewMutation.isPending.value"
              >
                {{ reviewForm.decision === 'approve' ? 'Approve' : 'Reject' }}
              </UButton>
            </div>
          </form>
        </UCard>
      </template>
    </UModal>
  </div>
</template>
