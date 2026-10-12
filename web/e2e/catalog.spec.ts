import { expect, test } from '@playwright/test'

// Acceptance scenarios from specs/001-book-catalog/spec.md, run against the real API + PostgreSQL.

test.describe('US1 — browse the catalog', () => {
  test('first page shows 12 books, the total and the page status', async ({ page }) => {
    await page.goto('/')

    await expect(page.getByTestId('catalog-result-count')).toHaveText('30 books')
    await expect(page.getByTestId('book-card')).toHaveCount(12)
    await expect(page.getByTestId('pagination-status')).toHaveText('Page 1 of 3')
  })

  test('Next moves to page 2 and the URL records it', async ({ page }) => {
    await page.goto('/')
    await page.getByTestId('pagination-next').click()

    await expect(page.getByTestId('pagination-status')).toHaveText('Page 2 of 3')
    await expect(page).toHaveURL(/page=2/)
  })

  test('an out-of-stock book is listed with its price and labelled', async ({ page }) => {
    await page.goto('/?search=dragon%20tattoo')

    const card = page.getByTestId('book-card')
    await expect(card).toHaveCount(1)
    await expect(card.getByTestId('book-card-stock')).toHaveText('Out of stock')
    await expect(card.getByTestId('book-card-price')).toHaveText('₹525.00')
  })
})

test.describe('US2 — search, filter and sort', () => {
  test('search matches author text in any case', async ({ page }) => {
    await page.goto('/')
    await page.getByTestId('catalog-search-input').fill('LE GUIN')
    await page.getByTestId('catalog-search-submit').click()

    await expect(page.getByTestId('catalog-result-count')).toHaveText('2 books')
    await expect(page).toHaveURL(/search=LE\+GUIN|search=LE%20GUIN/)
  })

  test('genre + price high-to-low puts the most expensive Technology book first', async ({ page }) => {
    await page.goto('/')
    await page.getByTestId('catalog-genre-select').selectOption('Technology')
    await page.getByTestId('catalog-sort-select').selectOption('price_desc')

    await expect(page.getByTestId('catalog-result-count')).toHaveText('5 books')
    await expect(page.getByTestId('book-card-title').first()).toHaveText('Domain-Driven Design')
    await expect(page.getByTestId('book-card-price').first()).toHaveText('₹1,450.00')
  })

  test('no matches shows the empty state and Clear filters restores the catalog', async ({ page }) => {
    await page.goto('/?search=zzzz-no-such-book')

    await expect(page.getByTestId('catalog-empty')).toContainText('No books match your search')
    await page.getByTestId('catalog-clear-filters').click()
    await expect(page.getByTestId('catalog-result-count')).toHaveText('30 books')
  })
})

test.describe('US3 — book details', () => {
  test('opening Dune shows ISBN, price and stock', async ({ page }) => {
    await page.goto('/?search=dune')
    await page.getByTestId('book-card-title').filter({ hasText: 'Dune' }).click()

    await expect(page.getByTestId('book-detail-title')).toHaveText('Dune')
    await expect(page.getByTestId('book-detail-isbn')).toHaveText('9780001000001')
    await expect(page.getByTestId('book-detail-price')).toHaveText('₹599.00')
    await expect(page.getByTestId('book-detail-stock')).toContainText('In stock')
  })

  test('Back to catalog restores the filters the shopper came from', async ({ page }) => {
    await page.goto('/?genre=Fantasy&sort=price_asc')
    await page.getByTestId('book-card-title').first().click()
    await page.getByTestId('book-detail-back').click()

    await expect(page).toHaveURL(/genre=Fantasy/)
    await expect(page).toHaveURL(/sort=price_asc/)
    await expect(page.getByTestId('catalog-result-count')).toHaveText('4 books')
  })

  test('an unknown book id shows Book not found', async ({ page }) => {
    await page.goto('/books/99999')

    await expect(page.getByTestId('book-not-found')).toBeVisible()
  })
})
