import http from '../http-common';

class userService {
    systemAdminRegistration(data) {
        return http.post("/UsersAccount/systemadmin/registration", data);
    }

    userRegistration(data) {
        return http.post("/UsersAccount/user/registration", data);
    }

    login(data) {
        console.log(" login(data) " + data.email + " " + data.password);
        return http.post("/UsersAccount/login", data);
    }

    authorization() {
        return http.post("/UsersAccount/refreshaccesstoken");
    }

    logOut() {
        return http.post("/UsersAccount/logout");
    }

    sendMessage(data) {
        return http.post("/SentMessages/send-message", data);
    }

    getChats() {
        return http.get("/SentMessages/get-last-messages");
    }

    getChatByRequester(data) {
        const queryParams = new URLSearchParams(data).toString();
        const url = "/SentMessages/get-messages-from-requester?" + queryParams;
        console.log("url = " + url);
        return http.get(url);
    }

    getUsers() {
        return http.get("/SystemAdmins");
    }

    getUser(id) {
        return http.get("/SystemAdmins/" + id);
    }

    deleteUser(id) {
        return http.delete("/SystemAdmins/" + id);
    }

    test() {
        return http.get("/Test/GgetHeaders");
    }
}

export default new userService();