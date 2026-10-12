import { useEffect, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { getBook } from '../api/catalog'
import { ApiError, isAbort } from '../api/client'
import { messageFrom } from '../api/errors'
import type { BookDetail } from '../api/types'
import { useCart } from '../cart/CartContext'
import StatusMessage from '../components/StatusMessage'
import { formatInr } from '../lib/money'

type LoadState =
  | { status: 'loading' }
  | { status: 'not-found' }
  | { status: 'error' }
  | { status: 'ready'; book: BookDetail }

export default function BookDetailPage() {
  const { id } = useParams()
  const location = useLocation()
  const fromSearch = (location.state as { fromSearch?: string } | null)?.fromSearch ?? ''
  const backTo = fromSearch ? `/?${fromSearch}` : '/'

  const [state, setState] = useState<LoadState>({ status: 'loading' })
  const [reloadKey, setReloadKey] = useState(0)
  const { addOne } = useCart()
  const [cartMessage, setCartMessage] = useState<string | null>(null)
  const [adding, setAdding] = useState(false)

  async function onAddToCart(bookId: number) {
    setAdding(true)
    setCartMessage(null)
    try {
      await addOne(bookId)
      setCartMessage('Added to cart.')
    } catch (error) {
      setCartMessage(messageFrom(error, 'We could not add this book to your cart.'))
    } finally {
      setAdding(false)
    }
  }

  useEffect(() => {
    const bookId = Number(id)
    if (!Number.isInteger(bookId) || bookId <= 0) {
      setState({ status: 'not-found' })
      return
    }
    const controller = new AbortController()
    setState({ status: 'loading' })
    getBook(bookId, controller.signal)
      .then((book) => setState({ status: 'ready', book }))
      .catch((error: unknown) => {
        if (isAbort(error)) return
        setState(error instanceof ApiError && error.status === 404 ? { status: 'not-found' } : { status: 'error' })
      })
    return () => controller.abort()
  }, [id, reloadKey])

  return (
    <section className="book-detail">
      <Link to={backTo} className="back-link" data-testid="book-detail-back">
        ← Back to catalog
      </Link>

      {state.status === 'loading' && <StatusMessage testId="book-detail-loading">Loading…</StatusMessage>}

      {state.status === 'not-found' && (
        <StatusMessage testId="book-not-found" tone="error">
          Book not found.
        </StatusMessage>
      )}

      {state.status === 'error' && (
        <StatusMessage
          testId="book-detail-error"
          tone="error"
          action={{ label: 'Retry', onClick: () => setReloadKey((k) => k + 1), testId: 'book-detail-retry' }}
        >
          We could not load this book.
        </StatusMessage>
      )}

      {state.status === 'ready' && (
        <article className="book-detail-body">
          <h1 data-testid="book-detail-title">{state.book.title}</h1>
          <p className="book-detail-author">by {state.book.author}</p>
          <p className="book-detail-price" data-testid="book-detail-price">
            {formatInr(state.book.price)}
          </p>
          <p
            className={state.book.inStock ? 'stock in' : 'stock out'}
            data-testid="book-detail-stock"
          >
            {state.book.inStock ? `In stock (${state.book.stockQuantity})` : 'Out of stock'}
          </p>
          <div className="add-to-cart">
            <button
              type="button"
              className="button"
              disabled={!state.book.inStock || adding}
              onClick={() => onAddToCart(state.book.id)}
              data-testid="add-to-cart"
            >
              Add to cart
            </button>
            {cartMessage && (
              <span role="status" data-testid="add-to-cart-status">
                {cartMessage}
              </span>
            )}
          </div>
          <p className="book-detail-description">{state.book.description}</p>
          <dl className="book-detail-meta">
            <dt>ISBN</dt>
            <dd data-testid="book-detail-isbn">{state.book.isbn}</dd>
            <dt>Genre</dt>
            <dd>{state.book.genre}</dd>
            <dt>Published</dt>
            <dd>{state.book.publishedYear}</dd>
          </dl>
        </article>
      )}
    </section>
  )
}
