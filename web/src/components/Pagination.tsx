interface PaginationProps {
  page: number
  totalPages: number
  onChange: (page: number) => void
}

export default function Pagination({ page, totalPages, onChange }: PaginationProps) {
  if (totalPages <= 1) return null
  return (
    <nav className="pagination" aria-label="Pagination">
      <button
        type="button"
        className="button"
        disabled={page <= 1}
        onClick={() => onChange(page - 1)}
        data-testid="pagination-prev"
      >
        Previous
      </button>
      <span data-testid="pagination-status">
        Page {page} of {totalPages}
      </span>
      <button
        type="button"
        className="button"
        disabled={page >= totalPages}
        onClick={() => onChange(page + 1)}
        data-testid="pagination-next"
      >
        Next
      </button>
    </nav>
  )
}
