import { defineStore } from "pinia";

export const useUsersStore = defineStore("UserStore", {

    state: () => {
        return {
            user: {
                id: "",
                email: "",
                userName: "",
                role: ""
            }
        }
    },

    actions: {
        setUser(id, email, name, role) {
            this.user.id = id;
            this.user.email = email;
            this.user.userName = name;
            this.user.role = role;
        },
        logOut() {
            this.user = {
                id: "",
                email: "",
                userName: "",
                role: ""
            };
        }
    },

    getters: {
        getUser() {
            return this.user;
        },
        getRole() {
            return this.user.role;
        },
        isUserLoggedIn() {
            return this.user.email !== "";
        },
        isSystemAdmin() {
            return this.user.role === "SystemAdmin";
        }
    }
})
