import { request } from './client'
import type { OrderView } from './types'

export function placeOrder(cartId: string, customerEmail: string): Promise<OrderView> {
  return request<OrderView>('/api/v1/orders', {
    method: 'POST',
    body: JSON.stringify({ cartId, customerEmail }),
  })
}

export function getOrder(orderId: string, signal?: AbortSignal): Promise<OrderView> {
  return request<OrderView>(`/api/v1/orders/${orderId}`, { signal })
}
