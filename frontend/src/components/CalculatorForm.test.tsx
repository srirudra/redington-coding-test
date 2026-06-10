import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { CalculatorForm } from './CalculatorForm'
import type { CalculationResult } from '../api/types'

function jsonResponse(body: unknown, init: ResponseInit): Response {
  return new Response(JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
}

describe('CalculatorForm', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows client-side errors and does not call the API when inputs are empty', async () => {
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    expect(
      await screen.findByText('First probability is required.'),
    ).toBeInTheDocument()
    expect(
      screen.getByText('Second probability is required.'),
    ).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('rejects an out-of-range probability before calling the API', async () => {
    const fetchMock = vi.fn()
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '1.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    expect(
      await screen.findByText(
        'First probability must be between 0 and 1 inclusive.',
      ),
    ).toBeInTheDocument()
    expect(fetchMock).not.toHaveBeenCalled()
  })

  it('submits valid input and displays the result', async () => {
    const result: CalculationResult = {
      firstProbability: 0.5,
      secondProbability: 0.4,
      calculationType: 'CombinedWith',
      result: 0.2,
      calculatedAtUtc: '2026-06-10T10:19:53.0535573Z',
    }
    const fetchMock = vi
      .fn()
      .mockResolvedValue(jsonResponse(result, { status: 200 }))
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    expect(await screen.findByText('0.2')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledTimes(1)
    const [, options] = fetchMock.mock.calls[0]
    expect(JSON.parse(options.body)).toEqual({
      firstProbability: 0.5,
      secondProbability: 0.4,
      calculationType: 'CombinedWith',
    })
  })

  it('clears a previous result when a probability input changes', async () => {
    const result: CalculationResult = {
      firstProbability: 0.5,
      secondProbability: 0.4,
      calculationType: 'CombinedWith',
      result: 0.2,
      calculatedAtUtc: '2026-06-10T10:19:53.0535573Z',
    }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(jsonResponse(result, { status: 200 })),
    )
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))
    expect(await screen.findByText('0.2')).toBeInTheDocument()

    await user.type(screen.getByLabelText(/first probability/i), '1')

    expect(screen.queryByText('0.2')).not.toBeInTheDocument()
  })

  it('clears a previous result when the calculation type changes', async () => {
    const result: CalculationResult = {
      firstProbability: 0.5,
      secondProbability: 0.4,
      calculationType: 'CombinedWith',
      result: 0.2,
      calculatedAtUtc: '2026-06-10T10:19:53.0535573Z',
    }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(jsonResponse(result, { status: 200 })),
    )
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))
    expect(await screen.findByText('0.2')).toBeInTheDocument()

    await user.click(screen.getByRole('radio', { name: /either/i }))

    expect(screen.queryByText('0.2')).not.toBeInTheDocument()
  })

  it('sends the selected calculation type', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      jsonResponse(
        {
          firstProbability: 0.5,
          secondProbability: 0.4,
          calculationType: 'Either',
          result: 0.7,
          calculatedAtUtc: '2026-06-10T10:19:53Z',
        },
        { status: 200 },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('radio', { name: /either/i }))
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1))
    const [, options] = fetchMock.mock.calls[0]
    expect(JSON.parse(options.body).calculationType).toBe('Either')
    expect(await screen.findByText('0.7')).toBeInTheDocument()
  })

  it('surfaces server-side validation errors against the right field', async () => {
    const problem = {
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        SecondProbability: [
          'SecondProbability must be between 0 and 1 inclusive.',
        ],
      },
    }
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(jsonResponse(problem, { status: 400 })),
    )
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    expect(
      await screen.findByText(
        'SecondProbability must be between 0 and 1 inclusive.',
      ),
    ).toBeInTheDocument()
  })

  it('shows a friendly message when the API is unreachable', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockRejectedValue(new TypeError('Failed to fetch')),
    )
    const user = userEvent.setup()

    render(<CalculatorForm />)
    await user.type(screen.getByLabelText(/first probability/i), '0.5')
    await user.type(screen.getByLabelText(/second probability/i), '0.4')
    await user.click(screen.getByRole('button', { name: /calculate/i }))

    expect(
      await screen.findByText(/unable to reach the calculation service/i),
    ).toBeInTheDocument()
  })
})
