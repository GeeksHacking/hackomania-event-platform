<script setup lang="ts">
import {
  geeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpointQueryKey,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersAddEndpoint,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersDeleteEndpoint,
  useGeeksHackingPortalApiEndpointsOrganizersHackathonOrganizersListEndpoint,
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

const isAddModalOpen = ref(false)
const form = ref({
  userId: '',
  type: 'Volunteer' as 'Admin' | 'Volunteer',
})

function resetForm() {
  form.value = { userId: '', type: 'Volunteer' }
}

function openAddModal() {
  resetForm()
  isAddModalOpen.value = true
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
          <UButton
            size="xs"
            icon="i-lucide-plus"
            class="w-full sm:w-auto"
            @click="openAddModal"
          >
            Add Organizer
          </UButton>
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
      </div>
    </template>
  </UDashboardPanel>
</template>
