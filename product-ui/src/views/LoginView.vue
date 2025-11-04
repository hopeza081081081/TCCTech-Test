<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { RouterLink } from 'vue-router' // ‼️ Import สำหรับลิงก์

const authStore = useAuthStore()
const router = useRouter() // ‼️ ตัวจัดการการเปลี่ยนหน้า

const loginEmail = ref<string>('admin@test.com')
const loginPassword = ref<string>('Password123!')

async function handleLogin() {
  const success = await authStore.login(loginEmail.value, loginPassword.value)
  if (success) {
    // ‼️ ถ้า Login สำเร็จ ให้เด้งไปหน้า Products
    router.push({ name: 'products' }) 
  }
}
</script>

<template>
  <div class="row justify-content-center mt-5">
    <div class="col-md-6 col-lg-4">
      <div class="card shadow-sm">
        <div class="card-body">
          <h3 class="card-title text-center mb-4">กรุณา Login</h3>
          <form @submit.prevent="handleLogin">
            <div class="mb-3">
              <label for="email" class="form-label">Email</label>
              <input type="email" v-model="loginEmail" class="form-control" id="email" required>
            </div>
            <div class="mb-3">
              <label for="password" class="form-label">Password</label>
              <input type="password" v-model="loginPassword" class="form-control" id="password" required>
            </div>
            
            <div v-if="authStore.error" class="alert alert-danger">
              {{ authStore.error }}
            </div>
            
            <button type="submit" class="btn btn-success w-100" :disabled="authStore.loading">
              {{ authStore.loading ? 'กำลัง Login...' : 'Login' }}
            </button>
          </form>
          
          <div class="text-center mt-3">
            <RouterLink :to="{ name: 'register' }">ยังไม่มีบัญชี? ลงทะเบียนที่นี่</RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>