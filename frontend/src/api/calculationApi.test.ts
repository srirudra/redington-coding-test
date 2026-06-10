import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  ApiRequestError,
  ApiValidationError,
  calculate,
} from './calculationApi'
import type { CalculationRequest, CalculationResult } from './types'

const request: CalculationRequest = {
  firstProbability: 0.5,
  secondProbability: 0.4,
  calculationType: 'CombinedWith',
}

function jsonResponse(body: unknown, init: ResponseInit): Response {
  return new Response(JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
}

describe('calculate', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('posts the request and returns the result on success', async () => {
    const result: CalculationResult = {
      ...request,
      result: 0.2,
      calculatedAtUtc: '2026-06-10T10:19:53.0535573Z',
    }
    const fetchMock = vi
      .fn()
      .mockResolvedValue(jsonResponse(result, { status: 200 }))
    vi.stubGlobal('fetch', fetchMock)

    const actual = await calculate(request)

    expect(actual).toEqual(result)
    expect(fetchMock).toHaveBeenCalledTimes(1)
    const [url, options] = fetchMock.mock.calls[0]
    expect(url).toContain('/api/calculations')
    expect(options).toMatchObject({ method: 'POST' })
    expect(JSON.parse(options.body)).toEqual(request)
  })

  it('throws ApiValidationError with field errors on a 400 response', async () => {
    const problem = {
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        FirstProbability: ['FirstProbability must be between 0 and 1 inclusive.'],
      },
    }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(jsonResponse(problem, { status: 400 })),
    )

    await expect(calculate(request)).rejects.toMatchObject({
      name: 'ApiValidationError',
      errors: problem.errors,
    })
  })

  it('throws ApiRequestError on an unexpected status', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(jsonResponse({}, { status: 500 })),
    )

    await expect(calculate(request)).rejects.toBeInstanceOf(ApiRequestError)
  })

  it('throws ApiRequestError when the request cannot be sent', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockRejectedValue(new TypeError('Failed to fetch')),
    )

    await expect(calculate(request)).rejects.toBeInstanceOf(ApiRequestError)
  })

  it('propagates an abort error without wrapping it', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockRejectedValue(new DOMException('Aborted', 'AbortError')),
    )

    await expect(calculate(request)).rejects.toMatchObject({
      name: 'AbortError',
    })
  })

  it('exposes validation messages through the error instance', () => {
    const error = new ApiValidationError({
      errors: { SecondProbability: ['bad value'] },
    })

    expect(error).toBeInstanceOf(ApiValidationError)
    expect(error.errors.SecondProbability).toEqual(['bad value'])
  })
})
