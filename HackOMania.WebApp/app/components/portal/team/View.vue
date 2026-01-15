<script setup lang="ts">
import { ref, computed } from 'vue'
import type { HackOManiaApiEndpointsParticipantsHackathonTeamsGetMineResponse } from '~/api-client/models'

const props = defineProps<{
  team: HackOManiaApiEndpointsParticipantsHackathonTeamsGetMineResponse | null | undefined
  hackathonId: string | null
}>()

const hackathonIdRef = computed(() => props.hackathonId)
const teamIdRef = computed(() => props.team?.id ?? null)

// State
const isLeavingTeam = ref(false)
const isCreatingTeam = ref(true) // Default to create team view
const isEditingName = ref(false)
const isEditingDescription = ref(false)

// Form inputs
const newTeamName = ref('')
const newTeamDescription = ref('')
const joinCode = ref('')
const editedTeamName = ref('')
const editedTeamDescription = ref('')

// Mutations
const createTeamMutation = useCreateTeam(hackathonIdRef)
const updateTeamMutation = useUpdateTeam(hackathonIdRef, teamIdRef)
const leaveTeamMutation = useLeaveTeam(hackathonIdRef)
const joinTeamMutation = useJoinTeamByCode()

// Computed
const hasTeam = computed(() => !!props.team?.id)
const memberCount = computed(() => props.team?.members?.length ?? 0)

// Handlers
function handleCreateTeam() {
  if (!newTeamName.value.trim()) return
  createTeamMutation.mutate({
    name: newTeamName.value.trim(),
    description: newTeamDescription.value.trim() || undefined,
  }, {
    onSuccess() {
      newTeamName.value = ''
      newTeamDescription.value = ''
    },
  })
}

function handleJoinTeam() {
  if (!joinCode.value.trim()) return
  joinTeamMutation.mutate(joinCode.value.trim(), {
    onSuccess() {
      joinCode.value = ''
    },
  })
}

function handleLeaveTeam() {
  leaveTeamMutation.mutate(undefined, {
    onSuccess() {
      isLeavingTeam.value = false
    },
    onError(error) {
      console.error('Failed to leave team:', error)
    },
  })
}

function startEditingName() {
  editedTeamName.value = props.team?.name ?? ''
  isEditingName.value = true
}

function saveTeamName() {
  if (!editedTeamName.value.trim()) return
  updateTeamMutation.mutate({ name: editedTeamName.value.trim() }, {
    onSuccess() {
      isEditingName.value = false
    },
  })
}

function startEditingDescription() {
  editedTeamDescription.value = props.team?.description ?? ''
  isEditingDescription.value = true
}

function saveTeamDescription() {
  updateTeamMutation.mutate({ description: editedTeamDescription.value.trim() }, {
    onSuccess() {
      isEditingDescription.value = false
    },
  })
}

function copyJoinCode() {
  if (props.team?.joinCode) {
    navigator.clipboard.writeText(props.team.joinCode)
  }
}
</script>

<template>
  <div>
    <!-- STATE: Participant WITH a team -->
    <div v-if="hasTeam">
      <p>Has team: {{ team?.name }}</p>
    </div>

    <!-- STATE: Participant WITHOUT a team -->
    <div v-else>
      <p>No team yet</p>
    </div>
  </div>
</template>
