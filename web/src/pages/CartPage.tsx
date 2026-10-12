import { useState } from 'react'
import { Link } from 'react-router-dom'
import { MAX_QUANTITY } from '../api/cart'
import { messageFrom } from '../api/errors'
import { useCart } from '../cart/CartContext'
import StatusMessage from '../components/StatusMessage'
import { formatInr } from '../lib/money'

const QUANTITIES = Array.from({ length: MAX_QUANTITY }, (_, i) => i + 1)

export default function CartPage() {
  const { cart, setQuantity, remove } = useCart()
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function run(action: () => Promise<void>) {
    setBusy(true)
    setError(null)
    try {
      await action()
    } catch (e) {
      setError(messageFrom(e, 'We could not update your cart.'))
    } finally {
      setBusy(false)
    }
  }

  if (!cart || cart.items.length === 0) {
    return (
      <section className="cart">
        <h1>Your cart</h1>
        <StatusMessage testId="cart-empty">Your cart is empty.</StatusMessage>
        <Link to="/" className="back-link">
          ← Browse the catalog
        </Link>
      </section>
    )
  }

  return (
    <section className="cart">
      <h1>Your cart</h1>
      {error && (
        <StatusMessage testId="cart-error" tone="error">
          {error}
        </StatusMessage>
      )}
      <table className="cart-table">
        <thead>
          <tr>
            <th>Book</th>
            <th>Price</th>
            <th>Quantity</th>
            <th>Total</th>
            <th aria-label="Actions" />
          </tr>
        </thead>
        <tbody>
          {cart.items.map((line) => (
            <tr key={line.bookId} data-testid="cart-line">
              <td data-testid="cart-line-title">
                <Link to={`/books/${line.bookId}`}>{line.title}</Link>
              </td>
              <td>{formatInr(line.unitPrice)}</td>
              <td>
                <select
                  value={line.quantity}
                  disabled={busy}
                  onChange={(e) => run(() => setQuantity(line.bookId, Number(e.target.value)))}
                  aria-label={`Quantity of ${line.title}`}
                  data-testid="cart-line-quantity"
                >
                  {QUANTITIES.map((q) => (
                    <option key={q} value={q}>
                      {q}
                    </option>
                  ))}
                </select>
              </td>
              <td data-testid="cart-line-total">{formatInr(line.lineTotal)}</td>
              <td>
                <button
                  type="button"
                  className="button secondary"
                  disabled={busy}
                  onClick={() => run(() => remove(line.bookId))}
                  data-testid="cart-line-remove"
                >
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="cart-subtotal">
        Subtotal: <strong data-testid="cart-subtotal">{formatInr(cart.subtotal)}</strong>
      </p>
      <Link to="/checkout" className="button" data-testid="cart-checkout">
        Checkout
      </Link>
    </section>
  )
}
