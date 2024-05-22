import { createWebHistory, createRouter } from "vue-router";
import {useUsersStore} from "./stores/UsersStore.js";
import {useAuthStore} from "./stores/AuthStore.js";

const routes =  [
    {
        path: "/test",
        component: () => import("./views/Test.vue")
    },
    {
        path: "/login",
        alias: ["/log-in", "/sign-in"],
        name: "login",
        component: () => import("./views/LogIn.vue"),
        /*beforeEnter: (to, from, next) => {
            if (!useUsersStore().isUserLoggedIn) {
                next({ name: "profile" });
            } else {
                next();
            }
        },*/
    },
    {
        path: "/registration",
        name: "registration",
        component: () => import("./views/SignUp.vue"),
        /*beforeEnter: (to, from, next) => {
            console.log("123");
            if (!useUsersStore().isUserLoggedIn) {
                next({ name: "profile" });
            } else {
                next();
            }
        },*/
        children: [
            {
                path: "user",
                component: () => import("./views/SignUp.vue"),
            },
            {
                path: "admin",
                component: () => import("./views/SignUp.vue"),
            }
        ]
    },
    {
        path: "/profile",
        name: "profile",
        component: () => import("./views/Profile.vue")
    },
    {
        path: "/chats",
        name: "chats",
        component: () => import("./views/Chats.vue"),
    },
    {
        path: "/chat/:requester",
        name: "chat",
        component: () => import("./views/Chat.vue"),
    },
    {
        path: "/admin",
        /*beforeEnter: (to, from, next) => {
            if (useUsersStore().isUserLoggedIn && useUsersStore().getRole === "SystemAdmin") {
                next();
            } else {
                next({ name: "notFound" });
            }
        },*/
        children: [
            {
                path: "",
                alias: ["users"],
                name: "adminUsers",
                component: () => import("./views/Users.vue"),
            },
            {
                path: "user/:id",
                name: "adminUser",
                component: () => import("./views/User.vue"),
            },
        ],
    },
    {
        path: "/:catchAll(.*)",
        name: "notFound",
        component: () => import("./views/NotFound.vue")
    },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

router.beforeEach(async (to, from, next) => {
    if (!useAuthStore().hasInitialization) {
        await useAuthStore().authorization();
    }

    if (to.name !== "login" &&
        to.name !== "registration" &&
        to.name !== "notFound" &&
        !useUsersStore().isUserLoggedIn) {
        next({name: "login"});
    } else if ((to.name === "login" || to.name === "registration") && useUsersStore().isUserLoggedIn) {
        next({name: "profile"});
    } else next();
});

export default router;