import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: JSON.parse(localStorage.getItem('cart')) || [],
    isLoading: false,
    error: null,
    toastMessage: ''
  }),

  getters: {
    totalItems: (state) => {
      return state.items.reduce((sum, item) => sum + Number(item.quantity || 0), 0)
    },
    totalPrice: (state) => {
      return state.items.reduce((sum, item) => sum + (Number(item.price || 0) * Number(item.quantity || 0)), 0)
    },
    totalTax: () => 0,
    totalWithTax: (state) => {
      return state.items.reduce((sum, item) => sum + (Number(item.price || 0) * Number(item.quantity || 0)), 0)
    }
  },

  actions: {
    addItem(product, qty = 1) {
      if (!product || !product.id) return
      const existing = this.items.find(item => Number(item.id) === Number(product.id))
      
      if (existing) {
        existing.quantity += Number(qty)
      } else {
        this.items.push({
          id: product.id,
          name: product.name,
          price: Number(product.price),
          quantity: Number(qty),
          imageUrl: product.imageUrl,
          categoryName: product.categoryName
        })
      }
      this.saveCart()
      this.showToast(`¡"${product.name}" añadido al carrito!`)
    },

    removeItem(productId) {
      const item = this.items.find(i => Number(i.id) === Number(productId))
      const name = item ? item.name : 'Producto'
      this.items = this.items.filter(i => Number(i.id) !== Number(productId))
      this.saveCart()
      this.showToast(`"${name}" eliminado del carrito`)
    },

    updateQuantity(productId, quantity) {
      const item = this.items.find(item => Number(item.id) === Number(productId))
      if (item) {
        const newQty = Number(quantity)
        if (newQty <= 0) {
          this.removeItem(productId)
        } else {
          item.quantity = newQty
          this.saveCart()
        }
      }
    },

    clearCart() {
      this.items = []
      this.saveCart()
      this.showToast('El carrito ha sido vaciado')
    },

    saveCart() {
      localStorage.setItem('cart', JSON.stringify(this.items))
    },

    showToast(message) {
      this.toastMessage = message
      setTimeout(() => {
        if (this.toastMessage === message) {
          this.toastMessage = ''
        }
      }, 3500)
    }
  }
})