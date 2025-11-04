<script setup lang="ts">
import { ref } from "vue";
import { useAuthStore } from "@/stores/auth";
import { useRouter } from "vue-router";
import { RouterLink } from "vue-router";

const authStore = useAuthStore();
const router = useRouter();

const regEmail = ref<string>("");
const regStoreName = ref<string>("");
const regPassword = ref<string>("");
const regConfirmPassword = ref<string>("");
const localError = ref<string>("");

async function handleRegister() {
  // Reset errors
  localError.value = "";
  authStore.error = null;

  // 1. Validate ที่ Frontend ก่อน
  if (regPassword.value.length < 6) {
    localError.value = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร";
    return;
  }
  if (regPassword.value !== regConfirmPassword.value) {
    localError.value = "รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน";
    return;
  }

  // 2. เรียก Store
  const success = await authStore.register(regEmail.value, regPassword.value, regStoreName.value);
  if (success) {
    // 3. ถ้าสำเร็จ, เด้งไปหน้า Login
    alert("ลงทะเบียนสำเร็จ! กรุณา Login");
    router.push({ name: "login" });
  }
  // ถ้าไม่สำเร็จ, authStore.error จะแสดงผลเอง
}
</script>

<template>
  <div class="row justify-content-center mt-5">
    <div class="col-md-6 col-lg-4">
      <div class="card shadow-sm">
        <div class="card-body">
          <h3 class="card-title text-center mb-4">ลงทะเบียน</h3>
          <form @submit.prevent="handleRegister">
            <div class="mb-3">
              <label for="email" class="form-label">Email</label>
              <input type="email" v-model="regEmail" class="form-control" id="email" required />
            </div>
            <div class="mb-3">
              <label for="storeName" class="form-label">Store Name</label>
              <input type="storeName" v-model="regStoreName" class="form-control" id="storeName" required />
            </div>
            <div class="mb-3">
              <label for="password" class="form-label">Password</label>
              <input
                type="password"
                v-model="regPassword"
                class="form-control"
                id="password"
                required
              />
            </div>
            <div class="mb-3">
              <label for="confirmPassword" class="form-label">Confirm Password</label>
              <input
                type="password"
                v-model="regConfirmPassword"
                class="form-control"
                id="confirmPassword"
                required
              />
            </div>

            <div v-if="localError" class="alert alert-warning">
              {{ localError }}
            </div>

            <div v-if="authStore.error" class="alert alert-danger">
              {{ authStore.error }}
            </div>

            <button type="submit" class="btn btn-primary w-100" :disabled="authStore.loading">
              {{ authStore.loading ? "กำลังลงทะเบียน..." : "Register" }}
            </button>
          </form>

          <div class="text-center mt-3">
            <RouterLink :to="{ name: 'login' }">มีบัญชีแล้ว? กลับไป Login</RouterLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
