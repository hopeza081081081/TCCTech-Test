<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useProductStore } from "@/stores/products";
import VueBarcode from "@chenfengyuan/vue-barcode";
import type { Product } from "@/types";

const productStore = useProductStore();

const newProductCode = ref<string>("");
const productCodeError = ref<string>("");

// โหลดข้อมูลสินค้าเมื่อหน้านี้ถูกเปิด
onMounted(() => {
  productStore.fetchProducts();
});

const formattedProductCode = (code: string): string => {
  if (!code || code.length !== 16) return code;
  return [
    code.substring(0, 4),
    code.substring(4, 8),
    code.substring(8, 12),
    code.substring(12, 16),
  ].join("-");
};

watch(newProductCode, (newValue: string) => {
  const upperCaseValue = newValue.toUpperCase().replace(/[^A-Z0-9]/g, "");

  if (upperCaseValue.length > 16) {
    newProductCode.value = upperCaseValue.substring(0, 16);
  } else {
    newProductCode.value = upperCaseValue;
  }

  if (upperCaseValue.length > 0 && upperCaseValue.length < 16) {
    productCodeError.value = "รหัสสินค้าต้องมี 16 หลัก";
  } else {
    productCodeError.value = "";
  }
});

async function handleAddProduct() {
  if (newProductCode.value.length !== 16 || productCodeError.value) {
    productCodeError.value = "รหัสสินค้าต้องมี 16 หลัก";
    return;
  }

  const success = await productStore.addProduct(newProductCode.value);
  if (success) {
    newProductCode.value = "";
  }
}

function handleDeleteProduct(product: Product) {
  const isConfirmed = window.confirm(
    `ต้องการลบข้อมูล รหัสสินค้า ${formattedProductCode(product.productCode)} หรือไม่ ?`
  );
  if (isConfirmed) {
    productStore.deleteProduct(product.id);
  }
}
</script>

<template>
  <form @submit.prevent="handleAddProduct" class="card card-body mb-4 shadow-sm">
    <div class="row g-3 align-items-center">
      <div class="col-auto">
        <label for="productCode" class="col-form-label fw-bold">รหัสสินค้า</label>
      </div>
      <div class="col">
        <input
          type="text"
          id="productCode"
          v-model="newProductCode"
          class="form-control"
          :class="{ 'is-invalid': productCodeError }"
          placeholder="XXXX-XXXX-XXXX-XXXX"
          maxlength="16"
          required
        />
        <div v-if="productCodeError" class="invalid-feedback d-block">
          {{ productCodeError }}
        </div>
      </div>
      <div class="col-auto">
        <button
          type="submit"
          class="btn btn-primary"
          :disabled="!!productCodeError || newProductCode.length !== 16"
        >
          ADD
        </button>
      </div>
    </div>
  </form>

  <div v-if="productStore.error" class="alert alert-danger">
    {{ productStore.error }}
  </div>

  <div class="card shadow-sm">
    <div class="table-responsive">
      <table class="table table-striped table-hover align-middle mb-0">
        <thead class="table-info">
          <tr>
            <th scope="col">Id</th>
            <th scope="col">รหัสสินค้า (16 หลัก)</th>
            <th scope="col" style="min-width: 250px">บาร์โค้ดสินค้า</th>
            <th scope="col" class="text-center">Action</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="productStore.loading">
            <td colspan="4" class="text-center p-4">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
            </td>
          </tr>
          <tr v-else-if="productStore.products.length === 0">
            <td colspan="4" class="text-center p-4">ยังไม่มีข้อมูลสินค้า</td>
          </tr>
          <tr v-for="product in productStore.products" :key="product.id">
            <td>{{ product.id }}</td>
            <td style="font-family: 'Courier New', Courier, monospace; font-weight: bold">
              {{ formattedProductCode(product.productCode) }}
            </td>
            <td>
              <VueBarcode
                :key="product.id"
                :value="product.productCode"
                :height="50"
                :options="{ displayValue: true, format: 'CODE39' }"
              />
            </td>
            <td class="text-center">
              <button @click="handleDeleteProduct(product)" class="btn btn-danger btn-sm">
                ลบ
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style>
/* (Style จาก App.vue ย้ายมานี่ หรือจะไว้ที่ App.vue ก็ได้) */
.table-info {
  background-color: #00bfff !important;
  color: white !important;
}
.table-info th {
  color: white;
}
</style>
