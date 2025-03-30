"use strict";

// Kết nối SignalR với CallHub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/callHub")
    .build();

let fullName = document.getElementById("hiddenFullName").value;
let isCalled = false;

function loadCallList() {
    connection.invoke("GetCallList")
        .then(() => console.log("Danh sách cuộc gọi đã được tải từ server."))
        .catch(err => console.error("Lỗi khi gọi GetCallList:", err.toString()));
}

// Khởi động kết nối SignalR
document.addEventListener("DOMContentLoaded", function () {

    connection.start().then(() => {
        console.log("Kết nối thành công với Hub");

        // Lấy FullName từ hidden input (được truyền từ Razor View)
        console.log("Lấy FullName: ", fullName);  // Kiểm tra giá trị của fullName

        // Thực hiện lời gọi AddUserToGroup để thêm người dùng vào nhóm dựa trên role
        connection.invoke("AddUserToGroup", fullName).catch(function (err) {
            console.error("Lỗi khi thêm vào nhóm:", err.toString());
        });
        // Gửi yêu cầu lấy danh sách người dùng
        //connection.invoke("GetConnectedUsers").catch(function (err) {
        //    console.error("Lỗi khi lấy danh sách người dùng:", err.toString());
        //});
        loadCallList();

    }).catch(function (err) {
        console.error("Lỗi kết nối với Hub:", err.toString());
    });
});

let unreadCount = 0; // Biến lưu số lượng tin nhắn chưa đọc
const unreadBadge = document.createElement("div");
unreadBadge.id = "unread-badge";
document.getElementById("messageButton").appendChild(unreadBadge);

// Hàm cập nhật số lượng tin nhắn chưa đọc
function updateUnreadCount() {
    if (unreadCount > 0) {
        unreadBadge.textContent = unreadCount;
        unreadBadge.style.visibility = "visible";
    } else {
        unreadBadge.style.visibility = "hidden";
    }
}

connection.on("ReceiveMessage", (sender, message) => {
    const messageBox = document.getElementById("messages");

    // Tạo container cho tin nhắn mới
    const messageContainer = document.createElement("div");
    messageContainer.style.marginBottom = "10px";

    if (sender === "You") {
        // Tin nhắn của người dùng hiện tại (căn phải)
        messageContainer.style.textAlign = "right";
        messageContainer.innerHTML = `
            <div style="display: inline-block; max-width: 70%; background: #007bff; color: white; padding: 10px; border-radius: 10px; text-align: left;">
                <strong>${sender}:</strong> ${message}
            </div>
        `;
    } else {
        // Tin nhắn từ người khác (căn trái)
        messageContainer.style.textAlign = "left";
        messageContainer.innerHTML = `
            <div style="display: inline-block; max-width: 70%; background: #f1f1f1; color: black; padding: 10px; border-radius: 10px; text-align: left;">
                <strong>${sender}:</strong> ${message}
            </div>
        `;

        // Nếu hộp chat chưa mở, tăng số lượng tin nhắn chưa đọc
        if (!chatBox.classList.contains("show")) {
            unreadCount++;
            updateUnreadCount();
        }
    }

    messageBox.appendChild(messageContainer);
    messageBox.scrollTop = messageBox.scrollHeight;
});

// Hàm hiển thị tin nhắn tại client (dùng cả khi nhận và gửi)
function displayMessage(sender, message) {
    const messageBox = document.getElementById("messages");
    const displayName = sender.includes("@") ? sender.split("@")[0] : sender;

    const messageContainer = document.createElement("div");
    messageContainer.className = "message-container " + (sender === "You" ? "right" : "left");

    const avatar = document.createElement("div");
    avatar.className = "message-avatar " + (sender === "You" ? "you" : "other");
    avatar.textContent = sender === "You" ? "Y" : displayName.charAt(0).toUpperCase();

    const messageBubble = document.createElement("div");
    messageBubble.className = "message-bubble " + (sender === "You" ? "you" : "other");

    if (sender !== "You") {
        const senderLabel = document.createElement("div");
        senderLabel.className = "sender-label";
        senderLabel.textContent = displayName;
        messageBubble.appendChild(senderLabel);
    }

    const messageText = document.createElement("div");
    messageText.textContent = message;
    messageBubble.appendChild(messageText);

    messageContainer.appendChild(avatar);
    messageContainer.appendChild(messageBubble);

    messageBox.appendChild(messageContainer);
    messageBox.scrollTop = messageBox.scrollHeight;
}

// Hiển thị/ẩn khung chat
const chatBox = document.getElementById("chat-box");
const openChatButton = document.getElementById("messageButton");
const closeChatButton = document.getElementById("closeChat");

// Mở hộp chat
openChatButton.addEventListener("click", () => {
    chatBox.style.display = "block";
    setTimeout(() => {
        chatBox.classList.add("show"); // Thêm hiệu ứng mở
        unreadCount = 0; // Xóa số lượng tin nhắn chưa đọc
        updateUnreadCount();
    }, 10);
});

// Đóng hộp chat
closeChatButton.addEventListener("click", () => {
    chatBox.classList.remove("show");
    setTimeout(() => {
        chatBox.style.display = "none"; // Ẩn hộp chat sau khi hiệu ứng hoàn tất
    }, 300); // Thời gian khớp với thời lượng hiệu ứng trong CSS
});

// Xử lý khi gửi tin nhắn
document.getElementById("sendMessageButton").addEventListener("click", () => {
    const messageInput = document.getElementById("messageInput");
    const message = messageInput.value.trim();

    if (message) {
        displayMessage("You", message);
        connection.invoke("SendMessage", message)
            .then(() => messageInput.value = "")
            .catch(err => console.error(err));
    }
});

// Nhận danh sách khách hàng đang gọi từ server
connection.on("ReceiveConnectedUsers", function (connectedUsers) {
    const callList = document.getElementById("callList");
    callList.innerHTML = ""; // Xóa danh sách cũ

    connectedUsers.forEach(user => {
        const userElement = document.createElement("div");
        userElement.className = "call-item";
        userElement.style.cursor = "pointer";
        userElement.style.padding = "10px";
        userElement.style.borderBottom = "1px solid #ccc";

        // Hiển thị tên và trạng thái của user
        userElement.textContent = `${user.name} (${user.isBusy ? "Bận" : "Rảnh"})`;
        userElement.style.color = user.isBusy ? "red" : "green";

        // Sự kiện khi nhân viên nhấn vào khách hàng (chỉ khi khách hàng đang rảnh)
        if (!user.isBusy) {
            userElement.addEventListener("click", function () {
                connection.invoke("AcceptCall", user.name).catch(err => {
                    console.error("Lỗi khi chấp nhận cuộc gọi:", err.toString());
                });

                // Chuyển sang giao diện cuộc gọi
                window.location.href = `/Call/Call?room=${user.name}`;
            });
        }

        callList.appendChild(userElement);
    });
});


// Hàm chấp nhận cuộc gọi
async function acceptCall(callerName) {
    try {
        await connection.invoke("AcceptCall", callerName);
        window.location.href = `/Call/Call?room=${callerName}`;  // Điều hướng tới giao diện cuộc gọi
    } catch (err) {
        console.error("Lỗi khi chấp nhận cuộc gọi:", err.toString());
    }
}

// Xử lý khi nhấn nút "Kết thúc cuộc gọi"
document.getElementById("leave-btn").addEventListener("click", function () {
    console.log("Nút kết thúc cuộc gọi được nhấn");

    // Gọi hàm EndCall trên Hub và truyền tên người gọi (fullName)
    connection.invoke("EndCall", fullName)
        .then(() => {
            console.log("Cuộc gọi đã kết thúc.");

            // Lấy Role từ server để điều hướng
            connection.invoke("GetUserRole")
                .then(role => {
                    console.log("Role hiện tại là:", role);
                    if (role === "Customer") {
                        window.location.href = "/Customer/Home";
                    } else if (role === "Employee") {
                        //loadCallList();
                        window.location.href = "/Employee";
                    } else {
                        console.error("Role không xác định:", role);
                        window.location.href = "/"; // Trang mặc định
                    }
                })
                .catch(err => console.error("Lỗi khi lấy Role:", err.toString()));
        })
        .catch(function (err) {
            console.error("Lỗi khi kết thúc cuộc gọi:", err.toString());
        });
});



// Khi nhấn vào biểu tượng điện thoại để bắt đầu cuộc gọi
document.getElementById("callButton").addEventListener("click", function () {
    console.log("Nút gọi đã được nhấn, bắt đầu gọi với callerName:", fullName);
    isCalled = true;
    //startCall(fullName);

    /*connection.invoke("GetConnectedUsers", isCalled).catch(function (err) {
        console.error("Lỗi khi lấy danh sách người dùng:", err.toString());
    });*/
    connection.invoke("CallEmployeesByCustomer", fullName).catch(function (err) {
        console.error("Lỗi cuộc gọi:", err.toString());
    });

    window.location.href = `/Call/Call?room=${fullName}`;
});

