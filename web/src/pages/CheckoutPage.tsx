import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { messageFrom } from '../api/errors'
import { placeOrder } from '../api/orders'
import { useCart } from '../cart/CartContext'
import StatusMessage from '../components/StatusMessage'
import { formatInr } from '../lib/money'

export default function CheckoutPage() {
  const { cart, reset } = useCart()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (!cart || cart.items.length === 0) {
    return (
      <section className="checkout">
        <h1>Checkout</h1>
        <StatusMessage testId="cart-empty">Your cart is empty.</StatusMessage>
        <Link to="/">Browse the catalog</Link>
      </section>
    )
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!cart) return
    setSubmitting(true)
    setError(null)
    try {
      const order = await placeOrder(cart.id, email)
      reset() // the server deleted the cart
      navigate(`/orders/${order.id}`, { state: { order } })
    } catch (e) {
      setError(messageFrom(e, 'We could not place your order.'))
      setSubmitting(false)
    }
  }

  return (
    <section className="checkout">
      <h1>Checkout</h1>
      <p>
        {cart.itemCount} {cart.itemCount === 1 ? 'item' : 'items'} · Subtotal{' '}
        <strong data-testid="checkout-subtotal">{formatInr(cart.subtotal)}</strong>
      </p>
      {error && (
        <StatusMessage testId="checkout-error" tone="error">
          {error}
        </StatusMessage>
      )}
      <form onSubmit={onSubmit} className="checkout-form">
        <label htmlFor="checkout-email">Email for your order confirmation</label>
        <input
          id="checkout-email"
          type="email"
          required
          maxLength={254}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          data-testid="checkout-email"
        />
        <button type="submit" className="button" disabled={submitting} data-testid="checkout-submit">
          {submitting ? 'Placing order…' : 'Place order'}
        </button>
      </form>
    </section>
  )
}
