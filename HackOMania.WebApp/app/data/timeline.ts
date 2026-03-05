import type { ItineraryItem } from '~/data/preevent'

export const prehackItems: ItineraryItem[] = [
  { time: '9:30 AM', event: 'Registration & Check-in' },
  { time: '10:00 AM', event: 'Opening Ceremony' },
  { time: '10:45 AM', event: 'Sponsor Sharing' },
  { time: '11:15 AM', event: 'Challenge Statement Release' },
  { time: '12:30 PM', event: 'Workshops & Networking Lunch' },
  { time: '4:00 PM', event: 'Pre-hack Wrap-up' },
]

export const day1Items: ItineraryItem[] = [
  { time: '8:00 AM', event: 'Arrival & Breakfast' },
  { time: '9:00 AM', event: 'Hackathon Kickoff' },
  { time: '12:00 PM', event: 'Lunch' },
  { time: '2:00 PM', event: 'Mentor Sessions' },
  { time: '6:00 PM', event: 'Dinner' },
  { time: '9:00 PM', event: 'Evening Check-in' },
]

export const day2Items: ItineraryItem[] = [
  { time: '8:00 AM', event: 'Breakfast' },
  { time: '9:00 AM', event: 'Final Stretch' },
  { time: '12:00 PM', event: 'Submission Deadline' },
  { time: '1:00 PM', event: 'Lunch & Judging' },
  { time: '3:00 PM', event: 'Presentations' },
  { time: '5:00 PM', event: 'Awards & Closing' },
]
