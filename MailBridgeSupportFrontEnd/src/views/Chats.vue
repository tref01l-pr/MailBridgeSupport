<script setup>
import { computed, ref } from 'vue';
import { useChatsStore } from "../stores/ChatsStore.js";
import router from "../router.js";

const isLoading = ref(true);
const chats = ref([]);

(async () => {
  try {
    chats.value = await useChatsStore().getChats;
  } catch (error) {
    console.error('Error while fetching chats:', error);
  } finally {
    isLoading.value = false;
  }
})();

function goToChat(chat) {
  router.push({ name: 'chat', params: { requester: chat.requester } });
}

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
      <p>Chats</p>
      <ul class="messages-container">
        <li class="last-message" v-for="chat in chats" :key="chat.id" @click="goToChat(chat)">
          <p class="time-right">{{ chat.lastMessage.date }}</p>
          <h2>Requester {{ chat.lastMessage.requester }}</h2>
          <h3>{{ chat.lastMessage.from }}</h3>
          <h4 v-if="chat.lastMessage.subject">{{ chat.lastMessage.subject }}</h4>
          <h4 v-else>Subject: No subject</h4>
        </li>
      </ul>

    </div>
  </div>
</template>

<style src="../css/main.scss"></style>
