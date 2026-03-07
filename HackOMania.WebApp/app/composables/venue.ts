import type { MaybeRefOrGetter } from 'vue'
import { queryOptions, useMutation } from '@tanstack/vue-query'
import { toValue } from 'vue'

export interface VenueCheckInResponse {
  id: string
  checkInTime: string
  isCheckedIn: boolean
}

export interface VenueCheckOutResponse {
  id: string
  checkOutTime: string
  isCheckedIn: boolean
}

export interface VenueHistoryItem {
  checkInTime: string
  checkOutTime?: string | null
  isCheckedIn: boolean
}

export interface VenueHistoryResponse {
  participantId: string
  userId: string
  userName: string
  isCurrentlyCheckedIn: boolean
  history: VenueHistoryItem[]
}

export interface ParticipantCheckInDto {
  participantId: string
  userId: string
  userName: string
  isCurrentlyCheckedIn: boolean
  lastCheckInTime?: string | null
  lastCheckOutTime?: string | null
  totalCheckIns: number
}

export interface VenueAuditTrailItem {
  participantId: string
  userId: string
  userName: string
  action: string
  timestamp: string
}

export interface VenueOverviewResponse {
  participants: ParticipantCheckInDto[]
  auditTrail: VenueAuditTrailItem[]
}

export const venueOverviewQueries = {
  overview: (hackathonId: string) =>
    queryOptions({
      queryKey: ['hackathons', hackathonId, 'venue', 'overview'],
      refetchInterval: 15_000,
      async queryFn() {
        const { public: { api } } = useRuntimeConfig()
        return await $fetch<VenueOverviewResponse>(
          `${api}/organizers/hackathons/${hackathonId}/venue/overview`,
          {
            credentials: 'include',
          },
        )
      },
    }),
}

export function useCheckInMutation(hackathonId: MaybeRefOrGetter<string>) {
  return useMutation({
    async mutationFn(participantUserId: string) {
      const { public: { api } } = useRuntimeConfig()
      return await $fetch<VenueCheckInResponse>(
        `${api}/organizers/hackathons/${toValue(hackathonId)}/participants/${participantUserId}/venue/check-in`,
        {
          method: 'POST',
          credentials: 'include',
        },
      )
    },
  })
}

export function useCheckOutMutation(hackathonId: MaybeRefOrGetter<string>) {
  return useMutation({
    async mutationFn(participantUserId: string) {
      const { public: { api } } = useRuntimeConfig()
      return await $fetch<VenueCheckOutResponse>(
        `${api}/organizers/hackathons/${toValue(hackathonId)}/participants/${participantUserId}/venue/check-out`,
        {
          method: 'POST',
          credentials: 'include',
        },
      )
    },
  })
}

export const venueHistoryQueries = {
  participant: (hackathonId: string, participantUserId: string) =>
    queryOptions({
      queryKey: ['hackathons', hackathonId, 'venue', 'history', participantUserId],
      async queryFn() {
        const { public: { api } } = useRuntimeConfig()
        return await $fetch<VenueHistoryResponse>(
          `${api}/organizers/hackathons/${hackathonId}/participants/${participantUserId}/venue/history`,
          {
            credentials: 'include',
          },
        )
      },
    }),
}
