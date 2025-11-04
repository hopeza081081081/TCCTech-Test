// src/stores/auth.ts
import apiClient from "@/services/api";
import { defineStore } from "pinia";

interface AuthState {
  token: string | null;
  user: string | null;
  error: string | null;
  loading: boolean;
}

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    token: localStorage.getItem("token") || null,
    user: localStorage.getItem("user") || null,
    error: null,
    loading: false,
  }),
  getters: {
    isLoggedIn: (state) => !!state.token,
  },
  actions: {
    async login(email: string, password: string): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const response = await apiClient.post<{ token: string; email: string }>("/auth/login", {
          email,
          password,
        });
        const { token, email: userEmail } = response.data;

        this.token = token;
        this.user = userEmail;
        localStorage.setItem("token", token);
        localStorage.setItem("user", userEmail);

        return true;
      } catch (err) {
        this.error = "Email หรือ Password ไม่ถูกต้อง";
        return false;
      } finally {
        this.loading = false;
      }
    },
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem("token");
      localStorage.removeItem("user");
    },
    // ‼️ --- Action ใหม่: Register --- ‼️
    async register(email: string, password: string, storeName: string): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        await apiClient.post("/auth/register", { email, password, storeName });
        return true; // ลงทะเบียนสำเร็จ
      } catch (err: any) {
        this.error = err.response?.data || "เกิดข้อผิดพลาดในการลงทะเบียน";
        return false;
      } finally {
        this.loading = false;
      }
    },
  },
});
