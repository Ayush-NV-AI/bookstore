import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { createCart, getCart, removeItem, setItemQuantity } from '../api/cart'
import { ApiError } from '../api/client'
import type { CartView } from '../api/types'

const STORAGE_KEY = 'bookstore.cartId'

function readStoredId(): string | null {
  try {
    return window.localStorage.getItem(STORAGE_KEY)
  } catch {
    return null
  }
}

function writeStoredId(id: string | null) {
  try {
    if (id) window.localStorage.setItem(STORAGE_KEY, id)
    else window.localStorage.removeItem(STORAGE_KEY)
  } catch {
    // storage unavailable (private mode) — the cart still works for this page view
  }
}

interface CartContextValue {
  cart: CartView | null
  itemCount: number
  /** Adds one copy of a book (creates the cart on first use). Throws ApiError on 409/404. */
  addOne: (bookId: number) => Promise<void>
  /** Sets (not adds) the quantity of a line. Throws ApiError on 409/400. */
  setQuantity: (bookId: number, quantity: number) => Promise<void>
  remove: (bookId: number) => Promise<void>
  /** Forget the cart after checkout (the server has deleted it). */
  reset: () => void
}

const CartContext = createContext<CartContextValue | null>(null)

function isNotFound(error: unknown): boolean {
  return error instanceof ApiError && error.status === 404
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<CartView | null>(null)

  useEffect(() => {
    const storedId = readStoredId()
    if (!storedId) return
    const controller = new AbortController()
    getCart(storedId, controller.signal)
      .then(setCart)
      .catch((error: unknown) => {
        if (isNotFound(error)) writeStoredId(null) // stale id after checkout — start fresh next time
      })
    return () => controller.abort()
  }, [])

  const ensureCart = useCallback(async (): Promise<CartView> => {
    if (cart) return cart
    const created = await createCart()
    writeStoredId(created.id)
    setCart(created)
    return created
  }, [cart])

  const reset = useCallback(() => {
    writeStoredId(null)
    setCart(null)
  }, [])

  const addOne = useCallback(
    async (bookId: number) => {
      let current = await ensureCart()
      const existing = current.items.find((i) => i.bookId === bookId)?.quantity ?? 0
      try {
        setCart(await setItemQuantity(current.id, bookId, existing + 1))
      } catch (error) {
        const cartVanished = isNotFound(error) && error instanceof ApiError && !!error.problem?.detail?.startsWith('No cart')
        if (!cartVanished) throw error
        // The stored cart vanished on the server: start a new one and retry once.
        writeStoredId(null)
        current = await createCart()
        writeStoredId(current.id)
        setCart(await setItemQuantity(current.id, bookId, 1))
      }
    },
    [ensureCart],
  )

  const setQuantity = useCallback(
    async (bookId: number, quantity: number) => {
      if (!cart) return
      setCart(await setItemQuantity(cart.id, bookId, quantity))
    },
    [cart],
  )

  const remove = useCallback(
    async (bookId: number) => {
      if (!cart) return
      setCart(await removeItem(cart.id, bookId))
    },
    [cart],
  )

  const value = useMemo<CartContextValue>(
    () => ({ cart, itemCount: cart?.itemCount ?? 0, addOne, setQuantity, remove, reset }),
    [cart, addOne, setQuantity, remove, reset],
  )

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext)
  if (!context) throw new Error('useCart must be used inside <CartProvider>')
  return context
}
