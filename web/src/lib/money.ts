const inr = new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' })

/** Formats a server-provided INR amount. The client never calculates prices (Constitution IV). */
export function formatInr(amount: number): string {
  return inr.format(amount)
}
