<template>
  <div class="container">
    <div class="form-body row">
      <div class="col-sm-9 col-md-7 col-lg-5 mx-auto">
        <div class="card border-0 shadow rounded-3 my-5">
          <div class="card-body p-4 p-sm-5">
            <h5 class="card-title text-center mb-5 fw-light fs-5">Log In</h5>
            <form @submit.prevent="submitForm">

              <LogInWithEmail
                  :userEmail="userEmail"
                  :password="password"
                  @update:userEmail="userEmail = $event"
                  @update:password="password = $event"
              />


              <div class="d-grid mt-5">
                <button class="btn btn-primary btn-login text-uppercase fw-bold" type="submit">Log
                  in</button>
              </div>
              <hr class="my-4">
              <div class="d-grid mb-2">
                <button class="btn btn-google btn-login text-uppercase fw-bold" type="submit">
                  <i class="fab fa-google me-2"></i> Log in with Google
                </button>
              </div>
              <div class="d-grid">
                <button class="btn btn-facebook btn-login text-uppercase fw-bold" type="submit">
                  <i class="fab fa-facebook-f me-2"></i> Log in with Facebook
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from "vue";
import usersService from "../services/usersService.js";
import { useRouter } from "vue-router";
import LogInWithEmail from "../components/Main/LogInViewComponents/LogInWithEmail.vue";
import { useJwtStore } from "../stores/JwtStore.js";
import { useAuthStore } from "../stores/AuthStore.js";

const router = useRouter();
let userEmail = ref("");
let password = ref("");

watch(userEmail, (newValue, oldValue) => {
  console.log("userEmail", newValue, oldValue);
});

watch(password, (newValue, oldValue) => {
  console.log("password", newValue, oldValue);
});

function resetInputs() {
  userEmail.value = "";
  password.value = "";
}
async function submitForm(event) {
  event.preventDefault();
  if (!password.value) {
    alert("Password is required");
    return;
  }

  if (!userEmail.value && userEmail.value.includes("@")) {
    alert("Email is required");
    return;
  }
  console.log("userEmail", userEmail.value);
  console.log("password", password.value);
  const data = {
    email: userEmail.value,
    password: password.value,
  };

  try {
    const response = await useAuthStore().login(userEmail.value, password.value);
    await router.push({ name: "chats" });
  } catch (error) {
    console.error("Error:", error);
  }
}

</script>

<style>
.form-body {
  margin-bottom: 250px;
}

.type-of-form {
  color: blue;
}

.active-form {
  font-weight: bold;
  text-decoration: underline;
}

</style>
