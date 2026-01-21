<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'

const props = defineProps<{
  title: string
  teamCount: number
  selected: boolean
  titleHeight?: number
}>()

const emit = defineEmits<{
  select: []
  titleMounted: [height: number]
}>()

const titleRef = ref<HTMLElement | null>(null)

onMounted(() => {
  if (titleRef.value) {
    emit('titleMounted', titleRef.value.offsetHeight)
  }
})

watch(() => props.title, () => {
  setTimeout(() => {
    if (titleRef.value) {
      emit('titleMounted', titleRef.value.offsetHeight)
    }
  }, 0)
})
</script>

<template>
  <div
    class="shadow-sm rounded-lg overflow-hidden cursor-pointer"
    @click="emit('select')"
  >
    <div
      ref="titleRef"
      class="px-3 lg:px-4 py-3 lg:py-4 rounded-lg text-center min-h-12 lg:min-h-18 flex items-center justify-center"
      :class="selected ? 'bg-[#FF5B84]' : 'bg-[#FF5B84]/40'"
      :style="titleHeight ? { height: `${titleHeight}px` } : {}"
    >
      <span class="font-['Zalando_Sans_Expanded'] text-black uppercase text-base lg:text-2xl">
        {{ title }}
      </span>
    </div>
    <div class="bg-white py-6 lg:py-12 px-4 lg:px-6 text-center flex flex-col gap-4 lg:gap-8">
      <div class="font-['Zalando_Sans_Expanded'] font-bold text-4xl lg:text-7xl">
        {{ teamCount }}
      </div>
      <div class="font-['Zalando_Sans_Expanded'] text-xl lg:text-4xl">
        Teams
      </div>
    </div>
  </div>
</template>
