export const formatBolivianos = (value) => {
  const amount = Number(value) || 0
  return `Bs ${amount.toFixed(2)}`
}
