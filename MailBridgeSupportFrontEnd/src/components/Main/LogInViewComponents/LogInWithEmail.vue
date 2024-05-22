<script setup>
import { ref } from "vue";
import usersService from "../../../services/usersService.js";
import { useRouter } from "vue-router";

const props = defineProps({
  userEmail: String,
  password: String
})


</script>

<template>

  <div class="form-floating mb-3">
    <input class="form-control" id="floatingInput" :value="userEmail" @change="$emit('update:userEmail', $event.target.value)" placeholder="name@example.com">
    <label for="floatingInput">Email</label>
  </div>
  <div class="form-floating mb-3">
    <input type="password" class="form-control" id="floatingPassword" :value="password" @change="$emit('update:password', $event.target.value)" placeholder="Password">
    <label for="floatingPassword">Password</label>
  </div>


</template>

<style scoped>

</style>
