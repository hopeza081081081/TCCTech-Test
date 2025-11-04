// src/stores/products.ts
import apiClient from '@/services/api';
import type { Product } from '@/types';
import { defineStore } from 'pinia';
import { useAuthStore } from './auth';

interface ProductState {
  products: Product[];
  loading: boolean;
  error: string | null;
}

export const useProductStore = defineStore('products', {
  state: (): ProductState => ({
    products: [],
    loading: false,
    error: null
  }),
  actions: {
    async fetchProducts() {
      this.loading = true;
      this.error = null;
      try {
        const response = await apiClient.get<Product[]>('/products');
        this.products = response.data;
      } catch (err: any) {
        this.error = 'ไม่สามารถโหลดข้อมูลสินค้าได้';
        if (err.response && err.response.status === 401) {
          useAuthStore().logout(); // Token หมดอายุ
        }
      } finally {
        this.loading = false;
      }
    },
    async addProduct(productCode: string): Promise<boolean> {
      this.error = null;
      try {
        const response = await apiClient.post<Product>('/products', { productCode });
        this.products.push(response.data);
        return true;
      } catch (err: any) {
        this.error = err.response?.data || 'เกิดข้อผิดพลาดในการเพิ่มข้อมูล';
        return false;
      }
    },
    async deleteProduct(productId: number) {
      this.error = null;
      try {
        await apiClient.delete(`/products/${productId}`);
        this.products = this.products.filter(p => p.id !== productId);
      } catch (err) {
        this.error = 'เกิดข้อผิดพลาดในการลบข้อมูล';
      }
    }
  }
});