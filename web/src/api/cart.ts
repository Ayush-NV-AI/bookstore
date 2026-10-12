import { request } from './client'
import type { CartView } from './types'

export const MAX_QUANTITY = 10

export function createCart(): Promise<CartView> {
  return request<CartView>('/api/v1/carts', { method: 'POST' })
}

export function getCart(cartId: string, signal?: AbortSignal): Promise<CartView> {
  return request<CartView>(`/api/v1/carts/${cartId}`, { signal })
}

export function setItemQuantity(cartId: string, bookId: number, quantity: number): Promise<CartView> {
  return request<CartView>(`/api/v1/carts/${cartId}/items/${bookId}`, {
    method: 'PUT',
    body: JSON.stringify({ quantity }),
  })
}

export function removeItem(cartId: string, bookId: number): Promise<CartView> {
  return request<CartView>(`/api/v1/carts/${cartId}/items/${bookId}`, { method: 'DELETE' })
}
