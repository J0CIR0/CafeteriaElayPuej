<template>
  <div class="container mt-4">
    <h2 class="mb-4">Nuestro Menú</h2>
    
    <div v-if="productsStore.loading" class="text-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Cargando...</span>
      </div>
    </div>
    
    <div v-else>
      <div class="row">
        <div v-for="product in productsStore.products" :key="product.id" class="col-md-4 mb-4">
          <div class="card h-100">
            <div class="card-body">
              <span class="badge bg-secondary">{{ product.categoryName }}</span>
              <h5 class="card-title mt-2">{{ product.name }}</h5>
              <p class="card-text text-muted">{{ product.description }}</p>
              <p class="card-text" v-if="product.origin">
                <small>Origen: {{ product.origin }}</small>
              </p>
              <p class="card-text" v-if="product.flavorNotes">
                <small>{{ product.flavorNotes }}</small>
              </p>
              <p class="card-text">
                <strong>Stock: {{ product.stock }}</strong>
              </p>
              <h4 class="text-primary">${{ product.price.toFixed(2) }}</h4>
              <button 
                class="btn btn-primary w-100" 
                @click="addToCart(product)"
                :disabled="product.stock === 0"
              >
                {{ product.stock === 0 ? 'Agotado' : 'Añadir al Carrito' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useProductsStore } from '../stores/products'
import { useCartStore } from '../stores/cart'

const productsStore = useProductsStore()
const cartStore = useCartStore()

onMounted(async () => {
  await productsStore.fetchAvailableProducts()
  await productsStore.fetchCategories()
})

const addToCart = (product) => {
  cartStore.addItem(product)
}
</script>