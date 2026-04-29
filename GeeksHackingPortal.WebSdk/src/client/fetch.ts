export type RequestConfig<TData = unknown> = {
  baseURL?: string
  url?: string
  method?: 'GET' | 'PUT' | 'PATCH' | 'POST' | 'DELETE' | 'OPTIONS' | 'HEAD'
  params?: Record<string, unknown> | URLSearchParams
  data?: TData | FormData
  responseType?: 'arraybuffer' | 'blob' | 'document' | 'json' | 'text' | 'stream'
  signal?: AbortSignal
  headers?: HeadersInit
  credentials?: RequestCredentials
}

export type ResponseConfig<TData = unknown> = {
  data: TData
  status: number
  statusText: string
  headers: Headers
}

export type ResponseErrorConfig<TError = unknown> = TError

type ErrorResponsePayload = {
  message?: string
  reason?: string
  title?: string
  errors?: Record<string, string[] | string>
}

export class ClientResponseError<TError = unknown> extends Error {
  status: number
  statusCode: number
  responseStatusCode: number
  response: {
    status: number
    statusText: string
    data: TError
    headers: Headers
  }
  data: TError

  constructor(args: {
    status: number
    statusText: string
    data: TError
    headers: Headers
    message: string
  }) {
    super(args.message)
    this.name = 'ClientResponseError'
    this.status = args.status
    this.statusCode = args.status
    this.responseStatusCode = args.status
    this.data = args.data
    this.response = {
      status: args.status,
      statusText: args.statusText,
      data: args.data,
      headers: args.headers,
    }
  }
}

export type Client = <TResponseData, _TError = unknown, TRequestData = unknown>(
  config: RequestConfig<TRequestData>,
) => Promise<ResponseConfig<TResponseData>>

type ConfigurableClient = Client & {
  getConfig: typeof getConfig
  setConfig: typeof setConfig
}

let config: Partial<RequestConfig> = {}

export const getConfig = () => config

export const setConfig = (nextConfig: Partial<RequestConfig>) => {
  config = nextConfig
  return getConfig()
}

function headersToObject(headers?: HeadersInit) {
  const result: Record<string, string> = {}

  new Headers(headers).forEach((value, key) => {
    result[key] = value
  })

  return result
}

export const mergeConfig = <T extends RequestConfig>(...configs: Array<Partial<T>>): Partial<T> => {
  return configs.reduce<Partial<T>>((merged, current) => ({
    ...merged,
    ...current,
    headers: {
      ...headersToObject(merged.headers),
      ...headersToObject(current.headers),
    },
  }), {})
}

function getErrorMessage(status: number, statusText: string, data: unknown) {
  if (data && typeof data === 'object') {
    const errorPayload = data as ErrorResponsePayload
    if (typeof errorPayload.message === 'string' && errorPayload.message.trim())
      return errorPayload.message
    if (typeof errorPayload.reason === 'string' && errorPayload.reason.trim())
      return errorPayload.reason
    if (typeof errorPayload.title === 'string' && errorPayload.title.trim())
      return errorPayload.title

    const firstFieldError = Object.values(errorPayload.errors ?? {}).flatMap(value =>
      Array.isArray(value) ? value : [value],
    )[0]
    if (typeof firstFieldError === 'string' && firstFieldError.trim())
      return firstFieldError
  }

  return statusText || `Request failed with status ${status}`
}

function withQuery(url: string, params?: RequestConfig['params']) {
  if (!params)
    return url

  const searchParams = params instanceof URLSearchParams
    ? params
    : new URLSearchParams(
        Object.entries(params)
          .filter(([, value]) => value !== undefined)
          .map(([key, value]) => [key, value === null ? 'null' : String(value)]),
      )

  const query = searchParams.toString()
  return query ? `${url}${url.includes('?') ? '&' : '?'}${query}` : url
}

export const client = (async <TResponseData, _TError = unknown, TRequestData = unknown>(
  requestConfig: RequestConfig<TRequestData>,
): Promise<ResponseConfig<TResponseData>> => {
  const resolvedConfig = mergeConfig(getConfig(), requestConfig)
  const url = withQuery([resolvedConfig.baseURL, resolvedConfig.url].filter(Boolean).join(''), resolvedConfig.params)
  const headers = new Headers(resolvedConfig.headers)
  const isFormData = resolvedConfig.data instanceof FormData
  if (!isFormData && resolvedConfig.data !== undefined && !headers.has('content-type'))
    headers.set('content-type', 'application/json')

  const body = isFormData ? resolvedConfig.data : JSON.stringify(resolvedConfig.data)

  const response = await fetch(url, {
    credentials: resolvedConfig.credentials ?? 'same-origin',
    method: resolvedConfig.method?.toUpperCase(),
    body: resolvedConfig.method === 'GET' || resolvedConfig.method === 'HEAD' ? undefined : body,
    signal: resolvedConfig.signal,
    headers,
  })

  const data = [204, 205, 304].includes(response.status) || !response.body
    ? {}
    : await response.json()

  if (!response.ok) {
    throw new ClientResponseError({
      status: response.status,
      statusText: response.statusText,
      data,
      headers: response.headers,
      message: getErrorMessage(response.status, response.statusText, data),
    })
  }

  return {
    data: data as TResponseData,
    status: response.status,
    statusText: response.statusText,
    headers: response.headers,
  }
}) as ConfigurableClient

client.getConfig = getConfig
client.setConfig = setConfig

export default client
