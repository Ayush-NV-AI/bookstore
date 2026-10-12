import { useEffect, useState, type FormEvent } from 'react'
import { useSearchParams } from 'react-router-dom'
import { listBooks, listGenres } from '../api/catalog'
import { isAbort } from '../api/client'
import type { BookSummary, CatalogSort, PagedResult } from '../api/types'
import BookCard from '../components/BookCard'
import Pagination from '../components/Pagination'
import StatusMessage from '../components/StatusMessage'

const SORT_OPTIONS: { value: CatalogSort; label: string }[] = [
  { value: 'title', label: 'Title A–Z' },
  { value: 'price_asc', label: 'Price: low to high' },
  { value: 'price_desc', label: 'Price: high to low' },
  { value: 'newest', label: 'Newest first' },
]

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; result: PagedResult<BookSummary> }

function toSort(value: string | null): CatalogSort {
  return SORT_OPTIONS.some((o) => o.value === value) ? (value as CatalogSort) : 'title'
}

export default function CatalogPage() {
  // FR-010: the URL is the single source of truth for search, genre, sort and page.
  const [params, setParams] = useSearchParams()
  const search = params.get('search') ?? ''
  const genre = params.get('genre') ?? ''
  const sort = toSort(params.get('sort'))
  const page = Math.max(1, Number(params.get('page')) || 1)

  const [searchInput, setSearchInput] = useState(search)
  const [genres, setGenres] = useState<string[]>([])
  const [state, setState] = useState<LoadState>({ status: 'loading' })
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => setSearchInput(search), [search])

  useEffect(() => {
    const controller = new AbortController()
    listGenres(controller.signal)
      .then(setGenres)
      .catch((error: unknown) => {
        if (!isAbort(error)) setGenres([])
      })
    return () => controller.abort()
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    setState({ status: 'loading' })
    listBooks({ search, genre, sort, page }, controller.signal)
      .then((result) => setState({ status: 'ready', result }))
      .catch((error: unknown) => {
        if (isAbort(error)) return
        setState({ status: 'error', message: 'We could not load the catalog.' })
      })
    return () => controller.abort()
  }, [search, genre, sort, page, reloadKey])

  function update(next: Record<string, string>) {
    const merged = new URLSearchParams(params)
    for (const [key, value] of Object.entries(next)) {
      if (value) merged.set(key, value)
      else merged.delete(key)
    }
    if (!('page' in next)) merged.delete('page') // any filter change returns to page 1
    setParams(merged)
  }

  function onSubmit(event: FormEvent) {
    event.preventDefault()
    update({ search: searchInput.trim() })
  }

  function clearFilters() {
    setSearchInput('')
    setParams(new URLSearchParams())
  }

  return (
    <section className="catalog">
      <form className="catalog-controls" onSubmit={onSubmit} role="search">
        <input
          type="search"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          placeholder="Search title or author"
          maxLength={100}
          aria-label="Search title or author"
          data-testid="catalog-search-input"
        />
        <button type="submit" className="button" data-testid="catalog-search-submit">
          Search
        </button>
        <select
          value={genre}
          onChange={(e) => update({ genre: e.target.value })}
          aria-label="Genre"
          data-testid="catalog-genre-select"
        >
          <option value="">All genres</option>
          {genres.map((g) => (
            <option key={g} value={g}>
              {g}
            </option>
          ))}
        </select>
        <select
          value={sort}
          onChange={(e) => update({ sort: e.target.value === 'title' ? '' : e.target.value })}
          aria-label="Sort"
          data-testid="catalog-sort-select"
        >
          {SORT_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </form>

      {state.status === 'loading' && (
        <StatusMessage testId="catalog-loading">Loading books…</StatusMessage>
      )}

      {state.status === 'error' && (
        <StatusMessage
          testId="catalog-error"
          tone="error"
          action={{ label: 'Retry', onClick: () => setReloadKey((k) => k + 1), testId: 'catalog-retry' }}
        >
          {state.message}
        </StatusMessage>
      )}

      {state.status === 'ready' && (
        <>
          <p className="result-count" data-testid="catalog-result-count">
            {state.result.totalCount} {state.result.totalCount === 1 ? 'book' : 'books'}
          </p>
          {state.result.items.length === 0 ? (
            <StatusMessage
              testId="catalog-empty"
              action={{ label: 'Clear filters', onClick: clearFilters, testId: 'catalog-clear-filters' }}
            >
              No books match your search.
            </StatusMessage>
          ) : (
            <div className="book-grid">
              {state.result.items.map((book) => (
                <BookCard key={book.id} book={book} fromSearch={params.toString()} />
              ))}
            </div>
          )}
          <Pagination
            page={state.result.page}
            totalPages={state.result.totalPages}
            onChange={(p) => update({ page: p > 1 ? String(p) : '' })}
          />
        </>
      )}
    </section>
  )
}
