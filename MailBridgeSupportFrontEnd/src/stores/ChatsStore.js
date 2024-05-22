import {defineStore} from "pinia";
import usersService from "../services/usersService.js";
import {useRoute, useRouter} from "vue-router";
import {useUsersStore} from "./UsersStore.js";


export const useChatsStore = defineStore("ChatsStore", {
    state: () => {
        return {
            chats: [
                {
                    requester: "",
                    lastMessage: {
                        requester: "",
                        from: "",
                        subject: "",
                        body: "",
                        date: "",
                        sentMessageStatus: "",
                    },
                    messages: [
                        {
                            requester: "",
                            from: "",
                            subject: "",
                            body: "",
                            date: "",
                            sentMessageStatus: "",
                        }
                    ]
                }
            ],
        }
    },
    actions: {
        async getChatsFromBackEnd() {
            try {
                const response = await usersService.getChats();
                this.chats = [];
                console.log("response.data")
                console.log(response.data);
                for (let i = 0; i < response.data.length; i++) {
                    this.chats.push({
                        requester: response.data[i].requester,
                        lastMessage: {
                            requester: response.data[i].requester,
                            from: response.data[i].from,
                            subject: response.data[i].subject,
                            body: response.data[i].body,
                            date: new Date(response.data[i].date).toISOString().slice(0, 19).replace("T", " "),
                            sentMessageStatus: response.data[i].sentMessageStatus,
                        },
                        messages: [
                            {
                                requester: "",
                                from: "",
                                subject: "",
                                body: "",
                                date: "",
                                sentMessageStatus: "",
                            }
                        ]
                    });
                }
            } catch (error) {
                console.log("Error: " + error);
            }
        },
        async getChatByRequesterFromBackEnd(requester, isChatCreated) {
            try {
                console.log("getChatByRequesterFromBackEnd !!!!!!!!!! isChatCreated = " + isChatCreated);
                const data = {
                    requester: requester
                }
                const response = await usersService.getChatByRequester(data);
                console.log(response.data);
                let localMessages = [];
                for (let i = 0; i < response.data.length; i++) {
                    localMessages.push({
                        requester: response.data[i].requester,
                        from: response.data[i].from,
                        subject: response.data[i].subject,
                        body: response.data[i].body,
                        date: response.data[i].date,
                        sentMessageStatus: response.data[i].sentMessageStatus,
                    });
                }

                let chat;

                if (isChatCreated) {
                    chat = this.chats.find(chat => chat.requester === requester);
                    chat.messages = localMessages;
                } else {
                    this.chats = [];
                    chat = {
                        requester: response.data[0].requester,
                        lastMessage: {
                            requester: "",
                            from: "",
                            subject: "",
                            body: "",
                            date: "",
                            sentMessageStatus: "",
                        },
                        messages: localMessages
                    }
                    this.chats.push(chat);
                }

                console.log(chat);
                return chat;
            } catch (error) {
                console.log("Error: " + error);
            }
        },
        async sendMessage(data) {
            try {
                console.log("async sendMessage(data)");
                console.log(this.chats);
                let chat = this.chats.find(chat => chat.requester === data.to);
                console.log(chat);
                if (chat === undefined) {
                    return;
                }

                let user = useUsersStore().getUser;
                let message = {
                    requester: data.to,
                    from: user.email,
                    subject: data.subject,
                    body: data.body,
                    date: new Date().toISOString().slice(0, 19).replace("T", " "),
                    sentMessageStatus: 1
                }

                chat.messages.unshift(message);
                chat.lastMessage = message;
                console.log("await usersService.sendMessage(data);");
                await usersService.sendMessage(data);
            } catch (error) {
                console.log("Error: " + error);
            }
        }
    },
    getters: {
        async getChats() {
            console.log("async getChats()");
            console.log(this.chats);
            if (this.chats.length <= 1 || this.chats[0].lastMessage.requester === "") {
                console.log("if (this.chats.length === 0) {");
                await this.getChatsFromBackEnd();
            }
            console.log("this.chats");
            console.log(this.chats);
            return this.chats;
        },
        async getChatByRequester() {
            const route = useRoute();
            const requester = route.params.requester;

            let result = this.chats.find(chat => chat.requester === requester);

            if (result === undefined) {
                result = await this.getChatByRequesterFromBackEnd(requester, false);
            }

            let resultMessages = result.messages;
            console.log("resultMessages");
            console.log(resultMessages);
            if (resultMessages === undefined || resultMessages.length === 0 || resultMessages[0].from === "") {
                console.log("if (resultMessages.length === 0) {");
                result = await this.getChatByRequesterFromBackEnd(requester, true);
            }
            console.log("this.chats");
            console.log(this.chats);
            return result;
        }
    },
});