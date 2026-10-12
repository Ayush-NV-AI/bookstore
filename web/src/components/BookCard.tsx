import { Link } from 'react-router-dom'
import type { BookSummary } from '../api/types'
import { formatInr } from '../lib/money'

interface BookCardProps {
  book: BookSummary
  /** Current catalog query string, so "Back to catalog" can restore it. */
  fromSearch: string
}

export default function BookCard({ book, fromSearch }: BookCardProps) {
  return (
    <article className="book-card" data-testid="book-card" data-book-id={book.id}>
      <Link to={`/books/${book.id}`} state={{ fromSearch }} className="book-card-link">
        <h2 className="book-card-title" data-testid="book-card-title">
          {book.title}
        </h2>
      </Link>
      <p className="book-card-author">{book.author}</p>
      <p className="book-card-genre">{book.genre}</p>
      <p className="book-card-price" data-testid="book-card-price">
        {formatInr(book.price)}
      </p>
      <p className={book.inStock ? 'stock in' : 'stock out'} data-testid="book-card-stock">
        {book.inStock ? 'In stock' : 'Out of stock'}
      </p>
    </article>
  )
}
