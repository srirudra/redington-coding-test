import { describe, expect, it } from 'vitest'
import { PROBABILITY_MAX_LENGTH, validateProbability } from './validation'

describe('validateProbability', () => {
  it('accepts the inclusive lower bound', () => {
    expect(validateProbability('0', 'First probability')).toBeNull()
  })

  it('accepts the inclusive upper bound', () => {
    expect(validateProbability('1', 'First probability')).toBeNull()
  })

  it('accepts a value inside the range', () => {
    expect(validateProbability('0.5', 'First probability')).toBeNull()
  })

  it('accepts scientific notation', () => {
    expect(validateProbability('1.8e-2', 'First probability')).toBeNull()
  })

  it('trims surrounding whitespace before parsing', () => {
    expect(validateProbability('  0.25  ', 'First probability')).toBeNull()
  })

  it('rejects an empty value as required', () => {
    expect(validateProbability('', 'First probability')).toBe(
      'First probability is required.',
    )
  })

  it('rejects a non-numeric value', () => {
    expect(validateProbability('abc', 'First probability')).toBe(
      'First probability must be a number.',
    )
  })

  it('rejects a value below the lower bound', () => {
    expect(validateProbability('-0.1', 'First probability')).toBe(
      'First probability must be between 0 and 1 inclusive.',
    )
  })

  it('rejects a value above the upper bound', () => {
    expect(validateProbability('1.1', 'Second probability')).toBe(
      'Second probability must be between 0 and 1 inclusive.',
    )
  })

  it('accepts input at the maximum allowed length', () => {
    const atLimit = '0.' + '0'.repeat(PROBABILITY_MAX_LENGTH - 2)
    expect(atLimit.length).toBe(PROBABILITY_MAX_LENGTH)
    expect(validateProbability(atLimit, 'First probability')).toBeNull()
  })

  it('rejects input longer than the maximum allowed length', () => {
    const tooLong = '0'.repeat(PROBABILITY_MAX_LENGTH + 1)
    expect(validateProbability(tooLong, 'First probability')).toBe(
      `First probability must be ${PROBABILITY_MAX_LENGTH} characters or fewer.`,
    )
  })
})
