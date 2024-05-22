<script setup>
import { computed } from "vue";
import { useUsersStore } from "../stores/UsersStore.js";

const userInformation = computed(() => useUsersStore().getUser);
</script>

<template>
  <div>
    <p>Profile</p>
    <p>id: {{userInformation.id}}</p>
    <p>email: {{userInformation.email}}</p>
    <p>userName: {{userInformation.userName}}</p>
    <p>role: {{userInformation.role}}</p>
  </div>
</template>