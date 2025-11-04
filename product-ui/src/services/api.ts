// src/services/api.ts
import { useAuthStore } from '@/stores/auth';
import axios, { type AxiosRequestConfig } from 'axios';

const apiClient = axios.create({
  baseURL: 'http://localhost:5090/api', // ‼️ แก้พอร์ต BE ให้ตรง
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor: แนบ Token
apiClient.interceptors.request.use(
  (config: AxiosRequestConfig) => {
    // ต้องเรียก store ภายใน function
    const authStore = useAuthStore(); 
    const token = authStore.token;
    if (token && config.headers) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default apiClient;