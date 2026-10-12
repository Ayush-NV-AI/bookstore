import { ApiError } from './client'

/** Human-readable message from an API failure: Problem Details detail, then first validation error, then title. */
export function messageFrom(error: unknown, fallback: string): string {
  if (error instanceof ApiError && error.problem) {
    const firstValidation = error.problem.errors ? Object.values(error.problem.errors).flat()[0] : undefined
    return error.problem.detail ?? firstValidation ?? error.problem.title ?? fallback
  }
  return fallback
}
