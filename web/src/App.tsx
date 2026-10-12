import { Link, Route, Routes } from 'react-router-dom'
import CatalogPage from './pages/CatalogPage'
import BookDetailPage from './pages/BookDetailPage'
import NotFoundPage from './pages/NotFoundPage'

export default function App() {
  return (
    <div className="app">
      <header className="app-header">
        <Link to="/" className="brand" data-testid="nav-home">
          Book Store
        </Link>
      </header>
      <main className="app-main">
        <Routes>
          <Route path="/" element={<CatalogPage />} />
          <Route path="/books/:id" element={<BookDetailPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </div>
  )
}
