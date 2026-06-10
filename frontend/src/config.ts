/**
 * Runtime configuration. The API base URL is read from the Vite env var
 * `VITE_API_BASE_URL` and falls back to the local backend default.
 */
export const apiBaseUrl: string =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:3000'
