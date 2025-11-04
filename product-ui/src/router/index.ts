// src/router/index.ts
import { useAuthStore } from '@/stores/auth'
import { createRouter, createWebHistory } from 'vue-router'

// 1. Import View Components (เรากำลังจะสร้างไฟล์เหล่านี้)
import LoginView from '@/views/LoginView.vue'
import ProductView from '@/views/ProductView.vue'
import RegisterView from '@/views/RegisterView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView
    },
    {
      path: '/',
      name: 'products',
      component: ProductView,
      meta: { requiresAuth: true } // ‼️ บอกว่าหน้านี้ต้อง Login
    }
  ]
})

// 2. ‼️ Navigation Guard (สำคัญมาก) ‼️
// นี่คือโค้ดที่จะรัน "ก่อน" ที่จะเปลี่ยนไปหน้าใหม่ทุกครั้ง
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  // 2.1 ถ้าหน้านั้น "ต้อง Login" (มี meta.requiresAuth) แต่ยังไม่ได้ Login
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    // ให้เด้งไปหน้า Login
    next({ name: 'login' })
  } 
  // 2.2 ถ้าจะไปหน้า Login หรือ Register "แต่" Login อยู่แล้ว
  else if ((to.name === 'login' || to.name === 'register') && authStore.isLoggedIn) {
    // ให้เด้งไปหน้า Products (/)
    next({ name: 'products' })
  } 
  // 2.3 กรณีอื่นๆ (ไปหน้าที่ต้อง Login และ Login แล้ว)
  else {
    // ไปต่อได้เลย
    next()
  }
})

export default router