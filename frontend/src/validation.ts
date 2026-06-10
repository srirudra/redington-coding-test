/**
 * Client-side probability validation. Mirrors the backend `[0, 1]` invariant
 * so users get immediate feedback; the server remains the source of truth.
 */

export const PROBABILITY_MIN = 0
export const PROBABILITY_MAX = 1

/**
 * Maximum number of characters accepted in a probability input. A valid
 * probability is short, so this is a generous guard against pathologically
 * long input (e.g. a large pasted string) before it reaches the backend.
 */
export const PROBABILITY_MAX_LENGTH = 128

/**
 * Validates a raw probability text input. Returns an error message when the
 * value is missing, too long, non-numeric, or outside the inclusive range
 * `[0, 1]`; returns `null` when valid.
 */
export function validateProbability(
  value: string,
  fieldLabel: string,
): string | null {
  if (value.length > PROBABILITY_MAX_LENGTH) {
    return `${fieldLabel} must be ${PROBABILITY_MAX_LENGTH} characters or fewer.`
  }

  const trimmed = value.trim()
  if (trimmed === '') {
    return `${fieldLabel} is required.`
  }

  const parsed = Number(trimmed)
  if (!Number.isFinite(parsed)) {
    return `${fieldLabel} must be a number.`
  }

  if (parsed < PROBABILITY_MIN || parsed > PROBABILITY_MAX) {
    return `${fieldLabel} must be between 0 and 1 inclusive.`
  }

  return null
}
