<template>
  <div class="admin-card">
    <div class="card-header">Generar Reportes</div>
    <div class="card-body">
      <div class="row g-3">
        <div class="col-md-4">
          <button class="btn btn-cafe w-100" @click="generateReport('products')">
            Productos
          </button>
        </div>
        <div class="col-md-4">
          <button class="btn btn-verde w-100" @click="generateReport('orders')">
            Pedidos
          </button>
        </div>
        <div class="col-md-4">
          <button class="btn btn-primary w-100" @click="generateReport('inventory')">
            Inventario
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()

const generateReport = (type) => {
  const data = {
    products: adminStore.products,
    orders: adminStore.orders,
    inventory: adminStore.inventoryMovements
  }[type]

  if (!data || data.length === 0) {
    alert('No hay datos para generar el reporte')
    return
  }

  const reportContent = generateHTML(type, data)
  printReport(reportContent)
}

const generateHTML = (type, data) => {
  const date = new Date().toLocaleString()
  const title = {
    products: 'Reporte de Productos',
    orders: 'Reporte de Pedidos',
    inventory: 'Reporte de Inventario'
  }[type]

  let tableRows = ''
  
  if (type === 'products') {
    tableRows = data.map(p => `
      <tr>
        <td>${p.id}</td>
        <td>${p.name}</td>
        <td>${p.categoryName || 'Sin categoria'}</td>
        <td>$${p.price.toFixed(2)}</td>
        <td>${p.stock}</td>
        <td>${p.isAvailable ? 'Disponible' : 'No Disponible'}</td>
      </tr>
    `).join('')
  } else if (type === 'orders') {
    tableRows = data.map(o => `
      <tr>
        <td>${o.orderNumber}</td>
        <td>${o.user?.fullName || 'N/A'}</td>
        <td>$${o.total.toFixed(2)}</td>
        <td>${o.paymentStatus === 'paid' ? 'Pagado' : 'Pendiente'}</td>
        <td>${o.orderStatus}</td>
        <td>${new Date(o.createdAt).toLocaleDateString()}</td>
      </tr>
    `).join('')
  } else if (type === 'inventory') {
    tableRows = data.map(m => `
      <tr>
        <td>${m.product?.name || 'N/A'}</td>
        <td>${m.movementType}</td>
        <td>${m.quantity}</td>
        <td>${m.reason || 'N/A'}</td>
        <td>${new Date(m.createdAt).toLocaleString()}</td>
      </tr>
    `).join('')
  }

  return `
    <!DOCTYPE html>
    <html>
    <head>
      <title>${title}</title>
      <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        .header { text-align: center; border-bottom: 3px solid #6F4E37; padding-bottom: 20px; margin-bottom: 20px; }
        .header h1 { color: #6F4E37; margin: 0; }
        .header p { color: #888; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th { background: #6F4E37; color: white; padding: 10px; text-align: left; }
        td { padding: 8px 10px; border-bottom: 1px solid #ddd; }
        tr:nth-child(even) { background: #f9f6f2; }
        .footer { text-align: center; margin-top: 30px; color: #888; font-size: 12px; border-top: 1px solid #ddd; padding-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>Cafeteria Elay Puej</h1>
        <h2>${title}</h2>
        <p>Generado: ${date}</p>
      </div>
      <table>
        <thead>
          <tr>
            ${type === 'products' ? '<th>ID</th><th>Nombre</th><th>Categoria</th><th>Precio</th><th>Stock</th><th>Estado</th>' :
              type === 'orders' ? '<th>Pedido</th><th>Cliente</th><th>Total</th><th>Pago</th><th>Estado</th><th>Fecha</th>' :
              '<th>Producto</th><th>Tipo</th><th>Cantidad</th><th>Motivo</th><th>Fecha</th>'}
          </tr>
        </thead>
        <tbody>
          ${tableRows}
        </tbody>
      </table>
      <div class="footer">
        <p>Cafeteria Elay Puej - Santa Cruz de la Sierra, Bolivia</p>
        <p>Reporte generado automaticamente</p>
      </div>
    </body>
    </html>
  `
}

const printReport = (html) => {
  const printWindow = window.open('', '_blank', 'width=1000,height=800')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
    printWindow.focus()
    printWindow.print()
  } else {
    alert('Por favor, permite ventanas emergentes para generar el reporte')
  }
}
</script>