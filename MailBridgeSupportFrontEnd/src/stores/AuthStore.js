import {defineStore} from "pinia";
import usersService from "../services/usersService.js";
import {useUsersStore} from "./UsersStore.js";
import {useJwtStore} from "./JwtStore.js";

export const useAuthStore = defineStore("AuthStore", {
    state: () => {
        return {
            hasInitialization: false
        }
    },
    actions: {
        async authorization() {
            try {
                let jwt = localStorage.getItem("jwt-token");

                if (jwt === "undefined" || jwt === "null" || jwt === null || jwt === undefined) {
                    jwt = "";
                } else {
                    let token = {
                        token: jwt
                    }

                    await useJwtStore().setJwtToken(token);
                }

                const response = await usersService.authorization();
                await useJwtStore().setJwtToken(response.data.accessToken);
                await useJwtStore().setJwtLocalStorage(response.data.accessToken);
                await useUsersStore().setUser(
                    response.data.id,
                    response.data.email,
                    response.data.nickname,
                    response.data.role);

                this.hasInitialization = true;
                return true;
            } catch (error) {
                console.log("Error: " + error);
                this.hasInitialization = true;
                return true;
            }
        },
        async login(email, password) {
            try {
                const data = {
                    email: email,
                    password: password
                };
                const response = await usersService.login(data);
                await useJwtStore().setJwtToken(response.data.accessToken);
                await useJwtStore().setJwtLocalStorage(response.data.accessToken);
                await useUsersStore().setUser(
                    response.data.id,
                    response.data.email,
                    response.data.nickname,
                    response.data.role);
            } catch (error) {
                console.log(error);
            }
        },
        async systemAdminRegistration(data) {
            try {
                await usersService.systemAdminRegistration(data);
            } catch (error) {
                console.log(error);
            }
        },
        async userRegistration(data) {
            try {
                await usersService.userRegistration(data);
            } catch (error) {
                console.log(error);
            }
        },
        async logOut() {
            try {
                let jwt = useJwtStore().getJwt;
                await usersService.logOut().then(() => {
                    useUsersStore().logOut();
                    useJwtStore().logOut();
                });
            } catch (error) {
                console.log(error);
            }
        }
    },
    getters: {
        getHasInitialization() {
            return this.hasInitialization;
        }
    },
});