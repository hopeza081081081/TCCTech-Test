<script setup lang="ts">
import { useAuthStore } from "@/stores/auth";
import { useStoreInfo } from "@/stores/store_info";
import { onMounted } from "vue";

import { useRouter } from "vue-router";
import { RouterView } from "vue-router"; // ‼️ Import ตัวแสดงผลของ Router

const authStore = useAuthStore();
const storeInfoStore = useStoreInfo();
const router = useRouter();

onMounted(() => {
  storeInfoStore.fetchStoreInfo();
});

function handleLogout() {
  authStore.logout();
  // ‼️ เมื่อ Logout ให้เด้งไปหน้า Login
  router.push({ name: "login" });
}
</script>

<template>
  <div>
    <nav class="navbar navbar-dark bg-success mb-4">
      <div class="container-fluid">
        <span class="navbar-brand mb-0 h1">IT 06-1</span>

        <div v-if="authStore.isLoggedIn" class="d-flex align-items-center">
          <span v-if="storeInfoStore.storeInfo?.storeName != null" class="navbar-text me-3"> Login: {{ storeInfoStore.storeInfo?.storeName ?? '' }} </span>
          <button @click="handleLogout" class="btn btn-outline-light btn-sm">Logout</button>
        </div>
      </div>
    </nav>

    <div class="container">
      <RouterView />
    </div>
  </div>
</template>

<style>
body {
  background-color: #f8f9fa; /* สีพื้นหลังอ่อนๆ */
}
</style>
