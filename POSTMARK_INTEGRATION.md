# Hook/Event System with Postmark Email Integration

## Overview
This feature allows organizers to automatically send emails to participants when they are accepted or rejected during the review process. It also provides a batch email endpoint for manual email sending.

## Configuration

Add the following configuration to your `appsettings.json` or environment variables:

```json
{
  "Postmark": {
    "ServerToken": "your-postmark-server-token",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "HackOMania",
    "Enabled": true
  }
}
```

### Configuration Options

- **ServerToken**: Your Postmark API server token (required)
- **FromEmail**: The sender email address (required)
- **FromName**: The sender name that appears in emails (required)
- **Enabled**: Set to `false` to disable email sending (useful for testing)

## Features

### 1. Automatic Email Hooks

When an organizer reviews a participant (accept or reject), an email is automatically sent to the participant:

**API Endpoint**: `POST /organizers/hackathons/{hackathonId}/participants/{participantUserId}/review`

**Request Body**:
```json
{
  "decision": "accept",  // or "reject"
  "reason": "Optional message to the participant"
}
```

The hook will:
- Send an acceptance email if decision is "accept"
- Send a rejection email if decision is "reject"
- Include the optional reason message in the email
- Log errors but not fail the review process if email sending fails

### 2. Batch Email Sending

Organizers can manually send emails to multiple participants at once:

**API Endpoint**: `POST /organizers/hackathons/{hackathonId}/participants/batch-email`

**Request Body**:
```json
{
  "status": "All",  // Options: "All", "Accepted", "Rejected"
  "participantUserIds": []  // Optional: specific participant IDs to target
}
```

**Response**:
```json
{
  "totalEmailsSent": 10,
  "acceptedEmailsSent": 7,
  "rejectedEmailsSent": 3,
  "errors": []
}
```

#### Use Cases:
- Send emails to all accepted participants: `{ "status": "Accepted" }`
- Send emails to all rejected participants: `{ "status": "Rejected" }`
- Resend emails to specific participants: `{ "participantUserIds": ["guid1", "guid2"] }`
- Send emails to all reviewed participants: `{ "status": "All" }`

## Email Templates

### Acceptance Email
- Professional HTML template with green header
- Includes optional message from organizers
- Plain text version included for compatibility

### Rejection Email
- Professional HTML template with red header
- Includes optional reason for rejection
- Encourages applying for future events
- Plain text version included for compatibility

## Security

- All user-supplied content (names, hackathon names, reasons) is HTML-encoded to prevent XSS
- Email sending is fault-tolerant - failures are logged but don't break the review process
- Batch operations continue even if individual emails fail

## Testing

For local development or testing, you can disable email sending:

```json
{
  "Postmark": {
    "ServerToken": "test-token",
    "FromEmail": "test@example.com",
    "FromName": "Test",
    "Enabled": false
  }
}
```

When `Enabled` is `false`, the system will log email attempts but not send actual emails.

## Error Handling

- Email sending errors are logged but don't prevent participant reviews
- Batch operations are fault-tolerant - one failure won't stop others
- All errors are logged with participant email addresses for debugging

## Requirements

- Postmark account with valid server token
- Verified sender email address in Postmark
- .NET 10.0 or higher
- Postmark NuGet package v5.3.0 or higher
