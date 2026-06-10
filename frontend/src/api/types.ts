/**
 * Contract types mirroring the backend API
 * (`POST /api/calculations`). The calculation type is serialized as a
 * string enum ("CombinedWith" / "Either").
 */

export type CalculationType = 'CombinedWith' | 'Either'

export interface CalculationRequest {
  firstProbability: number
  secondProbability: number
  calculationType: CalculationType
}

export interface CalculationResult {
  firstProbability: number
  secondProbability: number
  calculationType: CalculationType
  result: number
  calculatedAtUtc: string
}

/**
 * RFC 7807 ValidationProblemDetails shape returned by the API on a
 * 400 response. `errors` maps a field name to its validation messages.
 */
export interface ValidationProblemDetails {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}
