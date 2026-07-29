<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4 no-print">
      <div>
        <h5 class="section-title mb-1 fw-bold text-white">Panel Financiero, Rentabilidad y Pérdidas</h5>
        <p style="color:white">Estadísticas completas de ventas, costos de producción, margen de ganancia por producto y pérdidas por insumos vencidos.</p>
      </div>
      <button class="btn btn-success d-flex align-items-center gap-2" @click="exportPdf">
        <i class="bi bi-file-earmark-pdf fs-5"></i>
        <span>Exportar Reporte PDF</span>
      </button>
    </div>

    <div class="print-header d-none d-print-block mb-4 text-center">
      <h2>CAFETERÍA ELAY PUEJ</h2>
      <h4>REPORTE EJECUTIVO FINANCIERO Y CONTROL DE PÉRDIDAS</h4>
      <p class="text-muted">Fecha de generación: {{ new Date().toLocaleString('es-ES') }}</p>
      <hr>
    </div>

    <div class="row g-3 mb-4">
      <div class="col-md-3">
        <div class="stat-card-rounded">
          <div class="stat-icon-circle bg-primary">
            <i class="bi bi-cash-stack"></i>
          </div>
          <div>
            <div class="small text-muted fw-bold">Ventas Totales</div>
            <div class="h4 fw-bold mb-0 text-dark">Bs. {{ Number(report.totalRevenue || 0).toFixed(2) }}</div>
            <div class="small text-muted">Ingresos acumulados</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="stat-card-rounded">
          <div class="stat-icon-circle bg-secondary">
            <i class="bi bi-boxes"></i>
          </div>
          <div>
            <div class="small text-muted fw-bold">Costo Producción</div>
            <div class="h4 fw-bold mb-0 text-dark">Bs. {{ Number(report.totalProductionCost || 0).toFixed(2) }}</div>
            <div class="small text-muted">Insumos consumidos</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="stat-card-rounded">
          <div class="stat-icon-circle bg-danger">
            <i class="bi bi-exclamation-triangle-fill"></i>
          </div>
          <div>
            <div class="small text-muted fw-bold">Pérdidas / Mermas</div>
            <div class="h4 fw-bold mb-0 text-dark">Bs. {{ Number(report.totalWasteLossCost || 0).toFixed(2) }}</div>
            <div class="small text-muted">Insumos vencidos</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="stat-card-rounded">
          <div class="stat-icon-circle bg-success">
            <i class="bi bi-graph-up-arrow"></i>
          </div>
          <div>
            <div class="small text-muted fw-bold">Ganancia Limpia</div>
            <div class="h4 fw-bold mb-0 text-success">Bs. {{ Number(report.totalNetProfit || 0).toFixed(2) }}</div>
            <div class="small text-muted">Resultado neto final</div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="report.expiringIngredients && report.expiringIngredients.length > 0" class="alert alert-warning mb-4 shadow-sm">
      <h6 class="fw-bold mb-2">
        <i class="bi bi-exclamation-triangle-fill text-warning me-1"></i>
        Alerta de Insumos Próximos a Vencer (Próximos 7 días)
      </h6>
      <ul class="mb-0 ps-3">
        <li v-for="exp in report.expiringIngredients" :key="exp.ingredientId">
          <strong>{{ exp.name }}</strong> - Stock: {{ exp.stockQuantity }} {{ exp.unitOfMeasure }} - Vence el {{ new Date(exp.expirationDate).toLocaleDateString('es-ES') }} ({{ exp.daysUntilExpiration <= 0 ? 'VENCIDO HOY' : 'Faltan ' + exp.daysUntilExpiration + ' días' }})
        </li>
      </ul>
    </div>

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
                <td>Bs. {{ Number(item.salePrice).toFixed(2) }}</td>
                <td class="text-danger">Bs. {{ Number(item.unitCost).toFixed(2) }}</td>
                <td class="text-success fw-bold">Bs. {{ (item.salePrice - item.unitCost).toFixed(2) }}</td>
                <td class="fw-bold">{{ item.totalQuantitySold }}</td>
                <td>Bs. {{ Number(item.totalRevenue).toFixed(2) }}</td>
                <td class="text-success fw-bold">Bs. {{ Number(item.totalProfit).toFixed(2) }}</td>
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

    <div class="admin-card mb-4">
      <div class="card-header bg-white py-3 border-0 d-flex justify-content-between align-items-center">
        <h6 class="fw-bold mb-0">Histórico de Pérdidas por Insumos Vencidos o Mermados</h6>
        <span class="badge bg-danger">Total Perdido: Bs. {{ Number(report.totalWasteLossCost || 0).toFixed(2) }}</span>
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
                <th>Pérdida Total (Bs.)</th>
                <th>Motivo</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="loss in report.wasteLosses" :key="loss.id">
                <td>{{ new Date(loss.createdAt).toLocaleString('es-ES') }}</td>
                <td><strong class="text-dark">{{ loss.ingredientName }}</strong></td>
                <td>{{ loss.quantity }} {{ loss.unitOfMeasure }}</td>
                <td>Bs. {{ Number(loss.unitCostAtTime).toFixed(2) }}</td>
                <td class="text-danger fw-bold">Bs. {{ Number(loss.totalCostLoss).toFixed(2) }}</td>
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
