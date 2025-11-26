<script setup lang="ts">
import { useQuery } from '@tanstack/vue-query'

const { data, error } = useQuery(authQueries.whoAmI)
</script>

<template>
  <span>{{ data }} {{ error }}</span>
</template>
