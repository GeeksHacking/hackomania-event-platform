type ValidationErrorValue = string[] | string
interface ValidationErrorBag extends Record<string, ValidationErrorValue> {}

interface ApiErrorPayload {
  message?: unknown
  Message?: unknown
  reason?: unknown
  Reason?: unknown
  title?: unknown
  Title?: unknown
  errors?: unknown
  Errors?: unknown
}

interface ApiErrorLike {
  message?: unknown
  data?: ApiErrorPayload
  response?: {
    data?: ApiErrorPayload
  }
  errors?: unknown
}

function isValidationErrorBag(value: unknown): value is ValidationErrorBag {
  if (!value || typeof value !== 'object' || Array.isArray(value))
    return false

  return Object.values(value).every(entry => typeof entry === 'string' || (Array.isArray(entry) && entry.every(item => typeof item === 'string')))
}

function normalizeValidationErrors(value: unknown): ValidationErrorBag {
  if (!value || typeof value !== 'object')
    return {}

  const candidate = value as {
    additionalData?: unknown
  }

  if (isValidationErrorBag(candidate.additionalData))
    return candidate.additionalData

  if (isValidationErrorBag(value))
    return value

  return {}
}

export function getApiErrorPayload(error: unknown): ApiErrorPayload | undefined {
  if (!error || typeof error !== 'object')
    return undefined

  const candidate = error as ApiErrorLike
  return candidate.response?.data ?? candidate.data
}

export function getApiValidationErrors(error: unknown): ValidationErrorBag {
  const payload = getApiErrorPayload(error)
  if (payload?.errors || payload?.Errors)
    return normalizeValidationErrors(payload.errors ?? payload.Errors)

  if (error && typeof error === 'object') {
    const candidate = error as ApiErrorLike
    return normalizeValidationErrors(candidate.errors)
  }

  return {}
}

export function getApiErrorMessage(error: unknown, fallback: string): string {
  const payload = getApiErrorPayload(error)
  if (payload) {
    if (typeof payload.message === 'string' && payload.message.trim())
      return payload.message
    if (typeof payload.Message === 'string' && payload.Message.trim())
      return payload.Message
    if (typeof payload.reason === 'string' && payload.reason.trim())
      return payload.reason
    if (typeof payload.Reason === 'string' && payload.Reason.trim())
      return payload.Reason
    if (typeof payload.title === 'string' && payload.title.trim())
      return payload.title
    if (typeof payload.Title === 'string' && payload.Title.trim())
      return payload.Title

    const firstFieldError = Object.values(getApiValidationErrors(error)).flatMap(value =>
      Array.isArray(value) ? value : [value],
    )[0]

    if (typeof firstFieldError === 'string' && firstFieldError.trim())
      return firstFieldError
  }

  if (error && typeof error === 'object') {
    const candidate = error as ApiErrorLike
    if (typeof candidate.message === 'string' && candidate.message.trim())
      return candidate.message
  }

  return fallback
}
