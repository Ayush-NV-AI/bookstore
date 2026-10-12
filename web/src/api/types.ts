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

// Mirrors specs/002-orders-cart/contracts/orders-cart.openapi.yaml

export interface CartLineView {
  bookId: number
  title: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface CartView {
  id: string
  items: CartLineView[]
  itemCount: number
  subtotal: number
}

export interface OrderLineView {
  bookId: number
  title: string
  unitPrice: number
  quantity: number
  lineTotal: number
}

export interface OrderView {
  id: string
  orderNumber: string
  status: 'Placed'
  customerEmail: string
  placedAt: string
  total: number
  lines: OrderLineView[]
}
