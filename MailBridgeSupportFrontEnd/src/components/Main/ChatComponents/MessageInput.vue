<script setup>
import { ref } from 'vue';
import { useRoute } from 'vue-router';
import { useChatsStore } from "../../../stores/ChatsStore.js";

let messageBody = ref('');
let messageSubject = ref('');

const props = defineProps({
  requester: String
})

async function sendMessage(event) {
  event.preventDefault();
  if (!messageBody.value) {
    alert('Message body is required');
    return;
  }

  if (!messageSubject.value) {
    alert('Message subject is required');
    return;
  }

  try {
    let data = {
      to: props.requester,
      body: messageBody.value,
      subject: messageSubject.value
    }

    messageBody.value = '';
    messageSubject.value = '';

    await useChatsStore().sendMessage(data);
    alert('Success! Message sent!');
  } catch (error) {
    console.error('Error while sending message:', error);
  }
}
</script>

<template>
  <form class="input-chat-form" @submit.prevent="sendMessage">
    <div class="textarea-inputs">
      <input class="input-chat-subject" type="text" v-model="messageSubject" placeholder="Subject"/>
      <textarea class="input-chat-body" type="text" v-model="messageBody" placeholder="Body"></textarea>
    </div>
    <button type="submit" class="input-chat-submit-button" style="--clr:#39FF14"><span>Send</span><i></i></button>
  </form>
</template>

<style src="../../../css/main.scss"></style>