<script setup lang="ts">
interface TemplateRow {
  key: string
  templateId: string
}

const props = withDefaults(defineProps<{
  modelValue?: Record<string, string>
  eventKind?: 'hackathon' | 'standalone'
}>(), {
  modelValue: () => ({}),
  eventKind: 'standalone',
})

const emit = defineEmits<{
  'update:modelValue': [value: Record<string, string>]
}>()

const knownTemplateKeys = [
  {
    key: 'participant.review.accepted',
    label: 'Participant accepted',
    description: 'Sent when an organizer approves a hackathon participant.',
    scope: 'hackathon',
  },
  {
    key: 'participant.review.rejected',
    label: 'Participant rejected',
    description: 'Sent when an organizer rejects a hackathon participant.',
    scope: 'hackathon',
  },
] as const

const templateVariables = [
  'participant_name',
  'participant_first_name',
  'participant_last_name',
  'participant_email',
  'hackathon_name',
  'hackathon_short_code',
  'hackathon_venue',
  'event_start_date_formatted',
  'event_end_date_formatted',
  'review_status',
  'reason',
  'has_reason',
]

const rows = ref<TemplateRow[]>([])

watch(
  () => props.modelValue,
  (value) => {
    rows.value = Object.entries(value ?? {}).map(([key, templateId]) => ({ key, templateId }))
  },
  { immediate: true },
)

const availableTemplateKeys = computed(() =>
  knownTemplateKeys.filter(item => item.scope === 'all' || item.scope === props.eventKind),
)

function emitRows() {
  emit(
    'update:modelValue',
    Object.fromEntries(
      rows.value
        .map(row => [row.key.trim().toLowerCase(), row.templateId.trim()] as const)
        .filter(([key, templateId]) => key && templateId),
    ),
  )
}

function addTemplate(key = '') {
  rows.value = [...rows.value, { key, templateId: '' }]
}

function removeTemplate(index: number) {
  rows.value = rows.value.filter((_, rowIndex) => rowIndex !== index)
  emitRows()
}

function useTemplateKey(key: string) {
  const existing = rows.value.find(row => row.key === key)
  if (existing)
    return

  addTemplate(key)
}
</script>

<template>
  <div class="space-y-3">
    <div class="grid gap-2">
      <div
        v-for="(row, index) in rows"
        :key="`${row.key}-${index}`"
        class="grid gap-2 rounded-md border border-(--ui-border) p-2 sm:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_auto]"
      >
        <UInput
          v-model="row.key"
          placeholder="event key"
          @blur="emitRows"
          @change="emitRows"
        />
        <UInput
          v-model="row.templateId"
          placeholder="Postmark template alias or ID"
          @blur="emitRows"
          @change="emitRows"
        />
        <UButton
          type="button"
          icon="i-lucide-trash-2"
          color="error"
          variant="ghost"
          :aria-label="`Remove template ${row.key || index + 1}`"
          @click="removeTemplate(index)"
        />
      </div>
    </div>

    <UButton
      type="button"
      size="xs"
      variant="outline"
      icon="i-lucide-plus"
      @click="addTemplate()"
    >
      Add template
    </UButton>

    <div class="grid gap-3 rounded-md border border-(--ui-border) bg-(--ui-bg-muted) p-3 lg:grid-cols-2">
      <div class="space-y-2">
        <p class="text-sm font-medium">
          Available event keys
        </p>
        <div class="space-y-2">
          <button
            v-for="item in availableTemplateKeys"
            :key="item.key"
            type="button"
            class="block w-full rounded-md border border-(--ui-border) bg-(--ui-bg) p-2 text-left transition-colors hover:border-primary"
            @click="useTemplateKey(item.key)"
          >
            <span class="block text-sm font-medium">{{ item.label }}</span>
            <span class="block font-mono text-xs text-primary">{{ item.key }}</span>
            <span class="block text-xs text-(--ui-text-muted)">{{ item.description }}</span>
          </button>
          <p
            v-if="availableTemplateKeys.length === 0"
            class="rounded-md border border-dashed border-(--ui-border) p-2 text-xs text-(--ui-text-muted)"
          >
            No notification templates are currently triggered for this event type.
          </p>
        </div>
      </div>

      <div class="space-y-2">
        <p class="text-sm font-medium">
          Postmark model keys
        </p>
        <div class="flex flex-wrap gap-1">
          <UBadge
            v-for="variable in templateVariables"
            :key="variable"
            variant="subtle"
            color="neutral"
            class="font-mono"
          >
            {{ variable }}
          </UBadge>
        </div>
      </div>
    </div>
  </div>
</template>
