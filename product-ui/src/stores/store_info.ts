// src/stores/products.ts
import apiClient from "@/services/api";
import type { StoreInfo } from "@/types";
import { defineStore } from "pinia";
import { useAuthStore } from "./auth";

interface StoreInfoState {
  storeInfo: StoreInfo | null;
  loading: boolean;
  error: string | null;
}

export const useStoreInfo = defineStore("storeInfo", {
  state: (): StoreInfoState => ({
    storeInfo: null,
    loading: false,
    error: null,
  }),
  actions: {
    async fetchStoreInfo() {
      this.loading = true;
      this.error = null;
      try {
        const response = await apiClient.get<StoreInfo>("/store/information");
        this.storeInfo = response.data;
      } catch (err: any) {
        this.error = "ไม่สามารถโหลดข้อมูลโกดังได้";
        if (err.response && err.response.status === 401) {
          useAuthStore().logout(); // Token หมดอายุ
        }
      } finally {
        this.loading = false;
      }
    },
  },
});
