// Mirrors specs/001-book-catalog/contracts/catalog.openapi.yaml

export interface BookSummary {
  id: number
  title: string
  author: string
  genre: string
  price: number
  inStock: boolean
}

export interface BookDetail extends BookSummary {
  isbn: string
  publishedYear: number
  stockQuantity: number
  description: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

export type CatalogSort = 'title' | 'price_asc' | 'price_desc' | 'newest'
