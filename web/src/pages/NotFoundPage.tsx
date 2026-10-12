import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  return (
    <section className="not-found" data-testid="page-not-found">
      <h1>Page not found</h1>
      <Link to="/">Go to the catalog</Link>
    </section>
  )
}
