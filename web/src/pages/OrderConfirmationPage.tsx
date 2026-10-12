import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { ApiError, isAbort } from '../api/client'
import { getOrder } from '../api/orders'
import type { OrderView } from '../api/types'
import StatusMessage from '../components/StatusMessage'
import { formatInr } from '../lib/money'

type LoadState = { status: 'loading' } | { status: 'not-found' } | { status: 'error' } | { status: 'ready'; order: OrderView }

export default function OrderConfirmationPage() {
  const { id } = useParams()
  const location = useLocation()
  const fromCheckout = (location.state as { order?: OrderView } | null)?.order
  const [state, setState] = useState<LoadState>(
    fromCheckout && fromCheckout.id === id ? { status: 'ready', order: fromCheckout } : { status: 'loading' },
  )

  useEffect(() => {
    if (!id || (fromCheckout && fromCheckout.id === id)) return
    const controller = new AbortController()
    getOrder(id, controller.signal)
      .then((order) => setState({ status: 'ready', order }))
      .catch((error: unknown) => {
        if (isAbort(error)) return
        setState(error instanceof ApiError && error.status === 404 ? { status: 'not-found' } : { status: 'error' })
      })
    return () => controller.abort()
  }, [id, fromCheckout])

  if (state.status === 'loading') return <StatusMessage testId="order-loading">Loading your order…</StatusMessage>
  if (state.status === 'not-found')
    return (
      <StatusMessage testId="order-not-found" tone="error">
        Order not found.
      </StatusMessage>
    )
  if (state.status === 'error')
    return (
      <StatusMessage testId="order-error" tone="error">
        We could not load this order.
      </StatusMessage>
    )

  const { order } = state
  return (
    <section className="order-confirmation">
      <h1>Thank you — your order is placed</h1>
      <dl className="book-detail-meta">
        <dt>Order number</dt>
        <dd data-testid="order-number">{order.orderNumber}</dd>
        <dt>Status</dt>
        <dd data-testid="order-status">{order.status}</dd>
        <dt>Email</dt>
        <dd>{order.customerEmail}</dd>
      </dl>
      <table className="cart-table">
        <tbody>
          {order.lines.map((line) => (
            <tr key={line.bookId} data-testid="order-line">
              <td>{line.title}</td>
              <td>
                {line.quantity} × {formatInr(line.unitPrice)}
              </td>
              <td>{formatInr(line.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="cart-subtotal">
        Total: <strong data-testid="order-total">{formatInr(order.total)}</strong>
      </p>
      <Link to="/">Continue shopping</Link>
    </section>
  )
}
