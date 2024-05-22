<template>
  <div class="container">
    <div class="form-body row">
      <div class="col-sm-9 col-md-7 col-lg-5 mx-auto">
        <div class="card border-0 shadow rounded-3 my-5">
          <div class="card-body p-4 p-sm-5">
            <p>Sign Up</p>
            <form @submit.prevent="submitForm">

              <div class="form-floating mb-3">
                <input class="form-control" id="floatingInputUsername" v-model="username" placeholder="userName">
                <label for="floatingInputUsername">Username</label>
              </div>

              <div class="form-floating mb-3">
                <input class="form-control" id="floatingInputEmail" v-model="email" placeholder="name@example.com">
                <label for="floatingInputEmail">Email</label>
              </div>

              <div class="form-floating mb-3">
                <input type="password" class="form-control" id="floatingPassword" v-model="password"
                       placeholder="Password">
                <label for="floatingPassword">Password</label>
              </div>

              <div class="form-floating mb-3">
                <input type="password" class="form-control" id="floatingConfirmPassword" v-model="confirmPassword"
                       placeholder="Password">
                <label for="floatingConfirmPassword">Confirm Password</label>
              </div>

              <div class="d-grid mt-5">
                <button class="btn btn-primary btn-login text-uppercase fw-bold" type="submit">
                  Sign Up
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
import {ref} from "vue";
import usersService from "../services/usersService.js";
import {useRoute, useRouter} from "vue-router";
import LogInWithEmail from "../components/Main/LogInViewComponents/LogInWithEmail.vue";

const route = useRoute();
const router = useRouter();
const username = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");

async function submitForm() {
  try {
    if (!password.value || password.value !== confirmPassword.value) {
      alert("The passwords don't match");
      return;
    }

    if (!username.value) {
      alert("Username is required");
      return;
    }

    if (!email.value) {
      alert("Email is required");
      return;
    }

    let registrationType = route.params.requester;

    const data = {
      nickname: username.value,
      email: email.value,
      password: password.value,
    };

    if (registrationType === "superAdmin") {
      await usersService.systemAdminRegistration(data);
    } else {
      await usersService.userRegistration(data);
    }
    alert("Success! User registered!");
    await router.push("/login");
  } catch (error) {
    alert("Error while registering:", error);
  }

}
</script>
