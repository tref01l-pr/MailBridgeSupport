<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import {useChatsStore} from "../stores/ChatsStore.js";
import MessageInput from "../components/Main/ChatComponents/MessageInput.vue";

const chatsStore = useChatsStore();

const route = useRoute();

const isLoading = ref(true);
const chat = ref({});

(async () => {
  try {
    chat.value = await chatsStore.getChatByRequester;
    console.log('chat', chat.value);
  } catch (error) {
    console.error('Error while fetching chats:', error);
  } finally {
    isLoading.value = false;
  }
})();
console.log('asdasoigejfaeosirguihjaoeirg');
</script>

<template>
  <div>
    <div v-if="isLoading">
      <div class="loader">
        <span></span>
        <span></span>
        <span></span>
        <span></span>
      </div>
    </div>
    <div v-else>
      <h1>Requester: {{ chat.requester }}</h1>
      <ul class="messages-container">
        <li class="message" v-for="message in chat.messages" :key="message.id" :class="{ 'darker': message.sentMessageStatus }">
          <p class="time-right">{{ new Date(message.date).toISOString().slice(0, 19).replace("T", " ") }}</p>
          <h2>{{ message.from }}</h2>
          <h3 v-if="message.subject">{{ message.subject }}</h3>
          <h3 v-else>Subject: No subject</h3>
          <p v-if="message.body">{{ message.body }}</p>
          <p v-else>Body: No Body</p>
        </li>
      </ul>

      <MessageInput
          :requester="chat.requester"
      />
    </div>
  </div>
</template>

<!--
<style scoped>

</style>-->

<style src="../css/main.scss"></style>
