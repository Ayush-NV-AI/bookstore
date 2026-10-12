import { Link, Route, Routes } from 'react-router-dom'
import { useCart } from './cart/CartContext'
import BookDetailPage from './pages/BookDetailPage'
import CartPage from './pages/CartPage'
import CatalogPage from './pages/CatalogPage'
import CheckoutPage from './pages/CheckoutPage'
import NotFoundPage from './pages/NotFoundPage'
import OrderConfirmationPage from './pages/OrderConfirmationPage'

export default function App() {
  const { itemCount } = useCart()
  return (
    <div className="app">
      <header className="app-header">
        <Link to="/" className="brand" data-testid="nav-home">
          Book Store
        </Link>
        <Link to="/cart" className="cart-link" data-testid="nav-cart-link">
          Cart (<span data-testid="nav-cart-count">{itemCount}</span>)
        </Link>
      </header>
      <main className="app-main">
        <Routes>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/books/:id" element={<BookDetailPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/orders/:id" element={<OrderConfirmationPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </div>
  )
}
