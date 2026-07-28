<template>
  <div>
    <!-- Header with Export PDF Action -->
    <div class="d-flex justify-content-between align-items-center mb-4 no-print">
      <div>
        <h5 class="section-title mb-1">Panel Financiero, Rentabilidad y Pérdidas</h5>
        <p class="text-muted small mb-0">Estadísticas completas de ventas, costos de producción, margen de ganancia por producto y pérdidas por insumos vencidos.</p>
      </div>
      <button class="btn btn-success d-flex align-items-center gap-2" @click="exportPdf">
        <span>📄 Exportar Reporte PDF</span>
      </button>
    </div>

    <!-- Printable Header (Visible only when printing/exporting to PDF) -->
    <div class="print-header d-none d-print-block mb-4 text-center">
      <h2>CAFETERÍA ELAY PUEJ</h2>
      <h4>REPORTE EJECUTIVO FINANCIERO Y CONTROL DE PÉRDIDAS</h4>
      <p class="text-muted">Fecha de generación: {{ new Date().toLocaleString('es-ES') }}</p>
      <hr>
    </div>

    <!-- KPI Summary Cards -->
    <div class="row g-3 mb-4">
      <div class="col-md-3">
        <div class="card border-0 shadow-sm text-white bg-primary h-100">
          <div class="card-body">
            <div class="small opacity-75">Ventas Totales</div>
            <div class="h3 fw-bold mb-0">Bs {{ Number(report.totalRevenue || 0).toFixed(2) }}</div>
            <div class="small mt-1">Ingresos por productos vendidos</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm text-white bg-secondary h-100">
          <div class="card-body">
            <div class="small opacity-75">Costo de Producción</div>
            <div class="h3 fw-bold mb-0">Bs {{ Number(report.totalProductionCost || 0).toFixed(2) }}</div>
            <div class="small mt-1">Costo total de insumos consumidos</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm text-white bg-danger h-100">
          <div class="card-body">
            <div class="small opacity-75">Pérdidas (Insumos Vencidos/Mermas)</div>
            <div class="h3 fw-bold mb-0">Bs {{ Number(report.totalWasteLossCost || 0).toFixed(2) }}</div>
            <div class="small mt-1">Ingredientes dados de baja</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card border-0 shadow-sm text-white bg-success h-100">
          <div class="card-body">
            <div class="small opacity-75">Ganancia Neta Limpia</div>
            <div class="h3 fw-bold mb-0">Bs {{ Number(report.totalNetProfit || 0).toFixed(2) }}</div>
            <div class="small mt-1">Ventas - Costos - Pérdidas</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Expiring Ingredients Warning Banner -->
    <div v-if="report.expiringIngredients && report.expiringIngredients.length > 0" class="alert alert-warning mb-4 shadow-sm">
      <h6 class="fw-bold mb-2">⚠️ Alerta de Insumos Próximos a Vencer (Próximos 7 días)</h6>
      <ul class="mb-0 ps-3">
        <li v-for="exp in report.expiringIngredients" :key="exp.ingredientId">
          <strong>{{ exp.name }}</strong> - Stock: {{ exp.stockQuantity }} {{ exp.unitOfMeasure }} - Vence el {{ new Date(exp.expirationDate).toLocaleDateString('es-ES') }} ({{ exp.daysUntilExpiration <= 0 ? '¡VENCIDO HOY!' : 'Faltan ' + exp.daysUntilExpiration + ' días' }})
        </li>
      </ul>
    </div>

    <!-- Product Profitability Table -->
    <div class="admin-card mb-4">
      <div class="card-header bg-white py-3 border-0">
        <h6 class="fw-bold mb-0">Rentabilidad y Ganancia por Producto</h6>
      </div>
      <div class="card-body p-0">
        <div v-if="loading" class="text-center py-4">
          <div class="spinner-border text-primary" role="status"></div>
        </div>
        <div v-else-if="!report.productProfitability || report.productProfitability.length === 0" class="text-center py-4 text-muted">
          No hay datos de productos disponibles.
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Categoría</th>
                <th>Precio Venta</th>
                <th>Costo Insumos</th>
                <th>Ganancia / Ud.</th>
                <th>Cant. Vendida</th>
                <th>Ventas Totales</th>
                <th>Ganancia Total</th>
                <th>Margen (%)</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in report.productProfitability" :key="item.productId">
                <td><strong class="text-dark">{{ item.productName }}</strong></td>
                <td><span class="badge bg-light text-dark border">{{ item.categoryName }}</span></td>
                <td>Bs {{ Number(item.salePrice).toFixed(2) }}</td>
                <td class="text-danger">Bs {{ Number(item.unitCost).toFixed(2) }}</td>
                <td class="text-success fw-bold">Bs {{ (item.salePrice - item.unitCost).toFixed(2) }}</td>
                <td class="fw-bold">{{ item.totalQuantitySold }}</td>
                <td>Bs {{ Number(item.totalRevenue).toFixed(2) }}</td>
                <td class="text-success fw-bold">Bs {{ Number(item.totalProfit).toFixed(2) }}</td>
                <td>
                  <span class="badge" :class="item.profitMarginPercent >= 50 ? 'bg-success' : 'bg-warning text-dark'">
                    {{ Number(item.profitMarginPercent).toFixed(1) }}%
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Waste & Losses History Table -->
    <div class="admin-card mb-4">
      <div class="card-header bg-white py-3 border-0 d-flex justify-content-between align-items-center">
        <h6 class="fw-bold mb-0">Histórico de Pérdidas por Insumos Vencidos o Mermados</h6>
        <span class="badge bg-danger">Total Perdido: Bs {{ Number(report.totalWasteLossCost || 0).toFixed(2) }}</span>
      </div>
      <div class="card-body p-0">
        <div v-if="!report.wasteLosses || report.wasteLosses.length === 0" class="text-center py-4 text-muted">
          No hay registro de pérdidas o mermas de insumos.
        </div>
        <div v-else class="table-responsive">
          <table class="table-modern">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Insumo Mermado</th>
                <th>Cantidad</th>
                <th>Costo Unitario</th>
                <th>Pérdida Total (Bs)</th>
                <th>Motivo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="loss in report.wasteLosses" :key="loss.id">
                <td>{{ new Date(loss.createdAt).toLocaleString('es-ES') }}</td>
                <td><strong class="text-dark">{{ loss.ingredientName }}</strong></td>
                <td>{{ loss.quantity }} {{ loss.unitOfMeasure }}</td>
                <td>Bs {{ Number(loss.unitCostAtTime).toFixed(2) }}</td>
                <td class="text-danger fw-bold">Bs {{ Number(loss.totalCostLoss).toFixed(2) }}</td>
                <td><span class="badge bg-warning text-dark">{{ loss.reason }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useAdminStore } from '../../stores/admin'

const adminStore = useAdminStore()
const loading = ref(false)

const report = computed(() => adminStore.financialReport || {})

onMounted(async () => {
  loading.value = true
  await adminStore.fetchFinancialReport()
  loading.value = false
})

const exportPdf = () => {
  window.print()
}
</script>

<style scoped>
@media print {
  .no-print {
    display: none !important;
  }
  .print-header {
    display: block !important;
  }
  body {
    background: white !important;
    font-size: 12px;
  }
  .admin-card {
    border: 1px solid #ccc !important;
    box-shadow: none !important;
    margin-bottom: 20px !important;
    page-break-inside: avoid;
  }
}
</style>
