import { apiBaseUrl } from '../config'
import type {
  CalculationRequest,
  CalculationResult,
  ValidationProblemDetails,
} from './types'

/**
 * Raised when the API rejects a request with `400 Bad Request` and
 * field-specific validation messages.
 */
export class ApiValidationError extends Error {
  readonly errors: Record<string, string[]>

  constructor(problem: ValidationProblemDetails) {
    super(problem.title ?? 'The request failed validation.')
    this.name = 'ApiValidationError'
    this.errors = problem.errors ?? {}
  }
}

/**
 * Raised for transport failures or unexpected (non-200, non-400) responses.
 */
export class ApiRequestError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'ApiRequestError'
  }
}

/**
 * Calls `POST /api/calculations` and returns the calculation result.
 *
 * @throws {ApiValidationError} when the API returns a 400 validation problem.
 * @throws {ApiRequestError} on transport failure or an unexpected response.
 */
export async function calculate(
  request: CalculationRequest,
  signal?: AbortSignal,
): Promise<CalculationResult> {
  let response: Response
  try {
    response = await fetch(`${apiBaseUrl}/api/calculations`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
      signal,
    })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw error
    }
    throw new ApiRequestError(
      'Unable to reach the calculation service. Check your connection and try again.',
    )
  }

  if (response.ok) {
    return (await response.json()) as CalculationResult
  }

  if (response.status === 400) {
    const problem = (await response
      .json()
      .catch(() => ({}))) as ValidationProblemDetails
    throw new ApiValidationError(problem)
  }

  throw new ApiRequestError(
    `The calculation service responded with an unexpected status (${response.status}).`,
  )
}
