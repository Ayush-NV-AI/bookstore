import { expect, test, type Page } from '@playwright/test'

// Acceptance scenarios from specs/002-orders-cart/spec.md. Each test gets a fresh browser context,
// so localStorage (and therefore the cart) starts empty.

async function openBook(page: Page, title: string) {
  await page.goto(`/?search=${encodeURIComponent(title)}`)
  await page.getByTestId('book-card-title').filter({ hasText: title }).first().click()
  await expect(page.getByTestId('book-detail-title')).toHaveText(title)
}

async function addToCart(page: Page, title: string) {
  await openBook(page, title)
  await page.getByTestId('add-to-cart').click()
  await expect(page.getByTestId('add-to-cart-status')).toHaveText('Added to cart.')
}

test.describe('US1 — build a cart', () => {
  test('Add to cart puts the book in the cart and updates the header count', async ({ page }) => {
    await page.goto('/')
    await expect(page.getByTestId('nav-cart-count')).toHaveText('0')

    await addToCart(page, 'Pride and Prejudice')

    await expect(page.getByTestId('nav-cart-count')).toHaveText('1')
    await page.getByTestId('nav-cart-link').click()
    await expect(page.getByTestId('cart-line')).toHaveCount(1)
    await expect(page.getByTestId('cart-line-title')).toHaveText('Pride and Prejudice')
  })

  test('changing the quantity recalculates totals on the server; removing empties the cart', async ({ page }) => {
    await addToCart(page, 'Pride and Prejudice')
    await page.getByTestId('nav-cart-link').click()

    await page.getByTestId('cart-line-quantity').selectOption('2')

    await expect(page.getByTestId('cart-line-total')).toHaveText('₹500.00')
    await expect(page.getByTestId('cart-subtotal')).toHaveText('₹500.00')
    await expect(page.getByTestId('nav-cart-count')).toHaveText('2')

    await page.getByTestId('cart-line-remove').click()
    await expect(page.getByTestId('cart-empty')).toContainText('Your cart is empty')
    await expect(page.getByTestId('nav-cart-count')).toHaveText('0')
  })

  test('a quantity above stock is rejected with the stock level', async ({ page }) => {
    await addToCart(page, 'Neuromancer') // 4 in stock
    await page.getByTestId('nav-cart-link').click()

    await page.getByTestId('cart-line-quantity').selectOption('5')

    await expect(page.getByTestId('cart-error')).toContainText('Only 4 copies')
    await expect(page.getByTestId('cart-line-quantity')).toHaveValue('1')
  })

  test('Add to cart is disabled for an out-of-stock book', async ({ page }) => {
    await openBook(page, 'The Girl with the Dragon Tattoo')

    await expect(page.getByTestId('add-to-cart')).toBeDisabled()
  })
})

test.describe('US2 + US3 — check out and see the confirmation', () => {
  test('placing an order shows the confirmation and empties the cart', async ({ page }) => {
    await addToCart(page, 'Pride and Prejudice')
    await page.getByTestId('nav-cart-link').click()
    await page.getByTestId('cart-checkout').click()

    await page.getByTestId('checkout-email').fill('asha@example.com')
    await page.getByTestId('checkout-submit').click()

    await expect(page.getByTestId('order-status')).toHaveText('Placed')
    await expect(page.getByTestId('order-total')).toHaveText('₹250.00')
    await expect(page.getByTestId('order-number')).toHaveText(/^BS-\d{8}-\d{6}$/)
    await expect(page.getByTestId('nav-cart-count')).toHaveText('0')

    // The confirmation page also works on a fresh load (US3-1).
    await page.reload()
    await expect(page.getByTestId('order-status')).toHaveText('Placed')
  })
})
