<script setup lang="ts">
import {
  geeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpointQueryKey,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersAddEndpoint,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersDeleteEndpoint,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpoint,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersInviteEndpoint,
} from '@geekshacking/portal-sdk/hooks'
import { useQueryClient } from '@tanstack/vue-query'
import { computed, ref } from 'vue'

const route = useRoute()
const toast = useToast()
const queryClient = useQueryClient()
const hackathonId = computed(() => (route.params.hackathonId as string | undefined) ?? '')

const { data: organizersData, isLoading } = useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpoint(
  hackathonId,
  { query: { enabled: computed(() => !!hackathonId.value) } },
)

const organizers = computed(() => organizersData.value?.organizers ?? [])

const addMutation = useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersAddEndpoint()
const deleteMutation = useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersDeleteEndpoint()
const inviteMutation = useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersInviteEndpoint()

const isAddModalOpen = ref(false)
const isInviteModalOpen = ref(false)
const form = ref({
  userId: '',
  type: 'Volunteer' as 'Admin' | 'Volunteer',
})
const inviteForm = ref({
  type: 'Volunteer' as 'Admin' | 'Volunteer',
  maxUses: null as number | null,
})
const generatedInviteCode = ref<string | null>(null)
const generatedInviteExpiresAt = ref<string | null>(null)
const generatedInviteMaxUses = ref<number | null>(null)

function resetForm() {
  form.value = { userId: '', type: 'Volunteer' }
}

function resetInviteForm() {
  inviteForm.value = { type: 'Volunteer', maxUses: null }
  generatedInviteCode.value = null
  generatedInviteExpiresAt.value = null
  generatedInviteMaxUses.value = null
}

function openAddModal() {
  resetForm()
  isAddModalOpen.value = true
}

function openInviteModal() {
  resetInviteForm()
  isInviteModalOpen.value = true
}

async function handleAdd() {
  if (!form.value.userId.trim()) {
    toast.add({ title: 'User ID is required', color: 'error' })
    return
  }

  try {
    await addMutation.mutateAsync({
      hackathonId: hackathonId.value,
      data: { userId: form.value.userId.trim(), type: form.value.type },
    })
    await queryClient.invalidateQueries({
      queryKey: geeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpointQueryKey(hackathonId.value),
    })
    isAddModalOpen.value = false
    resetForm()
    toast.add({ title: 'Organizer added', color: 'success' })
  }
  catch {
    toast.add({ title: 'Failed to add organizer', description: 'Please check the user ID and try again.', color: 'error' })
  }
}

async function handleGenerateInvite() {
  try {
    const result = await inviteMutation.mutateAsync({
      hackathonId: hackathonId.value,
      data: {
        type: inviteForm.value.type,
        maxUses: inviteForm.value.maxUses ?? undefined,
      },
    })
    generatedInviteCode.value = result?.code ?? null
    generatedInviteExpiresAt.value = result?.expiresAt ?? null
    generatedInviteMaxUses.value = result?.maxUses ?? null
  }
  catch {
    toast.add({ title: 'Failed to generate invite code', color: 'error' })
  }
}

function copyInviteCode() {
  if (generatedInviteCode.value) {
    navigator.clipboard.writeText(generatedInviteCode.value)
    toast.add({ title: 'Invite code copied!', color: 'success' })
  }
}

async function handleDelete(userId: string) {
  try {
    await deleteMutation.mutateAsync({ hackathonId: hackathonId.value, userId })
    await queryClient.invalidateQueries({
      queryKey: geeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpointQueryKey(hackathonId.value),
    })
    toast.add({ title: 'Organizer removed', color: 'success' })
  }
  catch {
    toast.add({ title: 'Failed to remove organizer', color: 'error' })
  }
}

const isSubmitting = computed(() => addMutation.isPending.value)
const isGeneratingInvite = computed(() => inviteMutation.isPending.value)
</script>

<template>
  <UDashboardPanel id="organizers">
    <template #header>
      <UDashboardNavbar title="Organizers">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>
    </template>

    <template #body>
      <div class="space-y-3">
        <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
          <UBadge
            variant="subtle"
            size="sm"
            class="w-fit"
          >
            {{ organizers.length }} total
          </UBadge>
          <div class="flex flex-col gap-2 sm:flex-row">
            <UButton
              size="xs"
              icon="i-lucide-link"
              variant="outline"
              class="w-full sm:w-auto"
              @click="openInviteModal"
            >
              Create Invite Link
            </UButton>
            <UButton
              size="xs"
              icon="i-lucide-plus"
              class="w-full sm:w-auto"
              @click="openAddModal"
            >
              Add Organizer
            </UButton>
          </div>
        </div>

        <div class="rounded-xl border border-(--ui-border) bg-(--ui-bg)">
          <div
            v-if="isLoading"
            class="px-4 py-4 text-sm text-(--ui-text-muted)"
          >
            Loading organizers...
          </div>

          <div
            v-else-if="!organizers.length"
            class="px-4 py-4 text-sm text-(--ui-text-muted)"
          >
            No organizers yet.
          </div>

          <div
            v-else
            class="divide-y divide-(--ui-border) px-4"
          >
            <div
              v-for="organizer in organizers"
              :key="organizer.userId"
              class="py-3 flex items-center justify-between gap-3"
            >
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium truncate">
                  {{ organizer.name }}
                </p>
                <p class="text-xs text-(--ui-text-muted) truncate">
                  {{ organizer.email }}
                </p>
              </div>
              <div class="flex items-center gap-2 shrink-0">
                <UBadge
                  :color="organizer.type === 'Admin' ? 'primary' : 'neutral'"
                  variant="subtle"
                  size="xs"
                >
                  {{ organizer.type }}
                </UBadge>
                <UButton
                  v-if="organizer.userId"
                  size="xs"
                  variant="ghost"
                  color="error"
                  icon="i-lucide-trash-2"
                  :loading="deleteMutation.isPending.value"
                  @click="handleDelete(organizer.userId)"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- Add Organizer Modal -->
        <UModal v-model:open="isAddModalOpen">
          <template #content>
            <UCard>
              <template #header>
                <div class="flex items-center justify-between">
                  <h3 class="text-base font-semibold">
                    Add Organizer
                  </h3>
                  <UButton
                    variant="ghost"
                    icon="i-lucide-x"
                    size="xs"
                    @click="isAddModalOpen = false"
                  />
                </div>
              </template>

              <form
                class="space-y-4"
                @submit.prevent="handleAdd"
              >
                <UFormField label="User ID">
                  <UInput
                    v-model="form.userId"
                    placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
                  />
                </UFormField>

                <UFormField label="Role">
                  <USelect
                    v-model="form.type"
                    :items="[{ label: 'Volunteer', value: 'Volunteer' }, { label: 'Admin', value: 'Admin' }]"
                  />
                </UFormField>

                <div class="flex justify-end gap-2">
                  <UButton
                    variant="ghost"
                    @click="isAddModalOpen = false"
                  >
                    Cancel
                  </UButton>
                  <UButton
                    type="submit"
                    :loading="isSubmitting"
                  >
                    Add
                  </UButton>
                </div>
              </form>
            </UCard>
          </template>
        </UModal>

        <!-- Create Invite Link Modal -->
        <UModal v-model:open="isInviteModalOpen">
          <template #content>
            <UCard>
              <template #header>
                <div class="flex items-center justify-between">
                  <h3 class="text-base font-semibold">
                    Create Invite Link
                  </h3>
                  <UButton
                    variant="ghost"
                    icon="i-lucide-x"
                    size="xs"
                    @click="isInviteModalOpen = false"
                  />
                </div>
              </template>

              <div class="space-y-4">
                <div
                  v-if="!generatedInviteCode"
                  class="space-y-4"
                >
                  <UFormField label="Role">
                    <USelect
                      v-model="inviteForm.type"
                      :items="[{ label: 'Volunteer', value: 'Volunteer' }, { label: 'Admin', value: 'Admin' }]"
                    />
                  </UFormField>

                  <UFormField
                    label="Max Uses"
                    description="Leave empty for unlimited uses."
                  >
                    <UInput
                      v-model.number="inviteForm.maxUses"
                      type="number"
                      placeholder="Unlimited"
                      :min="1"
                    />
                  </UFormField>

                  <div class="flex justify-end gap-2">
                    <UButton
                      variant="ghost"
                      @click="isInviteModalOpen = false"
                    >
                      Cancel
                    </UButton>
                    <UButton
                      :loading="isGeneratingInvite"
                      icon="i-lucide-link"
                      @click="handleGenerateInvite"
                    >
                      Generate
                    </UButton>
                  </div>
                </div>

                <div
                  v-else
                  class="space-y-4"
                >
                  <div class="rounded-lg border border-(--ui-border) bg-(--ui-bg-elevated) p-4 space-y-2">
                    <p class="text-xs text-(--ui-text-muted)">
                      Invite code
                    </p>
                    <div class="flex items-center gap-2">
                      <code class="flex-1 text-base font-mono font-semibold tracking-widest">{{ generatedInviteCode }}</code>
                      <UButton
                        size="xs"
                        variant="ghost"
                        icon="i-lucide-copy"
                        @click="copyInviteCode"
                      />
                    </div>
                    <p class="text-xs text-(--ui-text-muted)">
                      Expires: {{ generatedInviteExpiresAt ? new Date(generatedInviteExpiresAt).toLocaleString() : 'N/A' }}
                    </p>
                    <p class="text-xs text-(--ui-text-muted)">
                      Max uses: {{ generatedInviteMaxUses ?? 'Unlimited' }}
                    </p>
                  </div>

                  <p class="text-sm text-(--ui-text-muted)">
                    Share this code with anyone you want to add as an organizer. They can use it at the accept invite page.
                  </p>

                  <div class="flex justify-end gap-2">
                    <UButton
                      variant="ghost"
                      @click="resetInviteForm"
                    >
                      Generate Another
                    </UButton>
                    <UButton @click="isInviteModalOpen = false">
                      Done
                    </UButton>
                  </div>
                </div>
              </div>
            </UCard>
          </template>
        </UModal>
      </div>
    </template>
  </UDashboardPanel>
</template>

