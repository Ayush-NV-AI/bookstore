import { request } from './client'
import type { BookDetail, BookSummary, CatalogSort, PagedResult } from './types'

export interface CatalogQuery {
  search?: string
  genre?: string
  sort?: CatalogSort
  page?: number
  pageSize?: number
}

export function listBooks(query: CatalogQuery, signal?: AbortSignal): Promise<PagedResult<BookSummary>> {
  const params = new URLSearchParams()
  if (query.search) params.set('search', query.search)
  if (query.genre) params.set('genre', query.genre)
  if (query.sort && query.sort !== 'title') params.set('sort', query.sort)
  if (query.page && query.page > 1) params.set('page', String(query.page))
  if (query.pageSize) params.set('pageSize', String(query.pageSize))
  const qs = params.toString()
  return request<PagedResult<BookSummary>>(`/api/v1/catalog/books${qs ? `?${qs}` : ''}`, { signal })
}

export function getBook(id: number, signal?: AbortSignal): Promise<BookDetail> {
  return request<BookDetail>(`/api/v1/catalog/books/${id}`, { signal })
}

export function listGenres(signal?: AbortSignal): Promise<string[]> {
  return request<string[]>('/api/v1/catalog/genres', { signal })
}
