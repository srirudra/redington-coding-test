import { useState, type FormEvent } from 'react'
import {
  ApiRequestError,
  ApiValidationError,
  calculate,
} from '../api/calculationApi'
import type { CalculationResult, CalculationType } from '../api/types'
import { PROBABILITY_MAX_LENGTH, validateProbability } from '../validation'
import './CalculatorForm.css'

interface FieldErrors {
  firstProbability?: string
  secondProbability?: string
}

const calculationOptions: ReadonlyArray<{
  value: CalculationType
  label: string
  description: string
}> = [
  {
    value: 'CombinedWith',
    label: 'Combined with',
    description: 'P(A) × P(B)',
  },
  {
    value: 'Either',
    label: 'Either',
    description: 'P(A) + P(B) − P(A) × P(B)',
  },
]

/**
 * Maps server-side validation problem keys (PascalCase property names) to the
 * form field error slots used by the UI.
 */
function mapServerErrors(errors: Record<string, string[]>): FieldErrors {
  const fieldErrors: FieldErrors = {}
  for (const [key, messages] of Object.entries(errors)) {
    const message = messages[0]
    if (!message) {
      continue
    }
    const normalized = key.toLowerCase()
    if (normalized.startsWith('firstprobability')) {
      fieldErrors.firstProbability = message
    } else if (normalized.startsWith('secondprobability')) {
      fieldErrors.secondProbability = message
    }
  }
  return fieldErrors
}

export function CalculatorForm() {
  const [firstProbability, setFirstProbability] = useState('')
  const [secondProbability, setSecondProbability] = useState('')
  const [calculationType, setCalculationType] =
    useState<CalculationType>('CombinedWith')

  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<string | null>(null)
  const [result, setResult] = useState<CalculationResult | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  // A previously calculated result becomes stale as soon as any input or the
  // calculation type changes, so clear it (and any form-level error) on change.
  function clearStaleResult() {
    setResult(null)
    setFormError(null)
  }

  function handleFirstProbabilityChange(value: string) {
    setFirstProbability(value)
    clearStaleResult()
  }

  function handleSecondProbabilityChange(value: string) {
    setSecondProbability(value)
    clearStaleResult()
  }

  function handleCalculationTypeChange(value: CalculationType) {
    setCalculationType(value)
    clearStaleResult()
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const errors: FieldErrors = {
      firstProbability:
        validateProbability(firstProbability, 'First probability') ?? undefined,
      secondProbability:
        validateProbability(secondProbability, 'Second probability') ??
        undefined,
    }

    if (errors.firstProbability || errors.secondProbability) {
      setFieldErrors(errors)
      setFormError(null)
      setResult(null)
      return
    }

    setFieldErrors({})
    setFormError(null)
    setIsSubmitting(true)

    try {
      const calculationResult = await calculate({
        firstProbability: Number(firstProbability),
        secondProbability: Number(secondProbability),
        calculationType,
      })
      setResult(calculationResult)
    } catch (error) {
      setResult(null)
      if (error instanceof ApiValidationError) {
        setFieldErrors(mapServerErrors(error.errors))
        setFormError('Please correct the highlighted fields and try again.')
      } else if (error instanceof ApiRequestError) {
        setFormError(error.message)
      } else {
        setFormError('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form className="calculator" onSubmit={handleSubmit} noValidate>
      <div className="field">
        <label htmlFor="firstProbability">First probability (A)</label>
        <input
          id="firstProbability"
          name="firstProbability"
          type="number"
          inputMode="decimal"
          step="any"
          min={0}
          max={1}
          maxLength={PROBABILITY_MAX_LENGTH}
          placeholder="0 to 1"
          value={firstProbability}
          onChange={(event) => handleFirstProbabilityChange(event.target.value)}
          aria-invalid={fieldErrors.firstProbability ? true : undefined}
          aria-describedby={
            fieldErrors.firstProbability ? 'firstProbability-error' : undefined
          }
        />
        {fieldErrors.firstProbability && (
          <p id="firstProbability-error" className="field-error" role="alert">
            {fieldErrors.firstProbability}
          </p>
        )}
      </div>

      <div className="field">
        <label htmlFor="secondProbability">Second probability (B)</label>
        <input
          id="secondProbability"
          name="secondProbability"
          type="number"
          inputMode="decimal"
          step="any"
          min={0}
          max={1}
          maxLength={PROBABILITY_MAX_LENGTH}
          placeholder="0 to 1"
          value={secondProbability}
          onChange={(event) =>
            handleSecondProbabilityChange(event.target.value)
          }
          aria-invalid={fieldErrors.secondProbability ? true : undefined}
          aria-describedby={
            fieldErrors.secondProbability
              ? 'secondProbability-error'
              : undefined
          }
        />
        {fieldErrors.secondProbability && (
          <p id="secondProbability-error" className="field-error" role="alert">
            {fieldErrors.secondProbability}
          </p>
        )}
      </div>

      <fieldset className="field">
        <legend>Calculation type</legend>
        {calculationOptions.map((option) => (
          <label key={option.value} className="radio">
            <input
              type="radio"
              name="calculationType"
              value={option.value}
              checked={calculationType === option.value}
              onChange={() => handleCalculationTypeChange(option.value)}
            />
            <span>
              {option.label}{' '}
              <span className="radio-description">{option.description}</span>
            </span>
          </label>
        ))}
      </fieldset>

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Calculating…' : 'Calculate'}
      </button>

      {formError && (
        <p className="form-error" role="alert">
          {formError}
        </p>
      )}

      {result && (
        <div className="result" role="status" aria-live="polite">
          <span className="result-label">Result</span>
          <span className="result-value">{result.result}</span>
        </div>
      )}
    </form>
  )
}
