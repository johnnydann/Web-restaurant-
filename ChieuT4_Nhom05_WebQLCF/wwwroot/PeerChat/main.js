"use strict";

// Variables
let client;
let roomId = new URLSearchParams(window.location.search).get("room");
let localStream, remoteStream, peerConnection;

//let holdMusic = new Audio('~/wwwroot/PeerChat/nhaccho.mp3'); // Đường dẫn tới file nhạc
//holdMusic.loop = true; // Lặp lại nhạc


// STUN servers for WebRTC
const servers = {
    iceServers: [
        { urls: ["stun:stun1.l.google.com:19302", "stun:stun2.l.google.com:19302"] }
    ]
};

// Video/audio constraints
const constraints = {
    video: { width: { ideal: 1920 }, height: { ideal: 1080 } },
    audio: true
};

// Initialize connection and local video
const init = async () => {
    if (!roomId) {
        console.error("Room ID is missing");
        return;
    }

    try {
        // Initialize SignalR client
        client = new signalR.HubConnectionBuilder().withUrl("/callhub").build();
        await client.start();
        console.log("SignalR client started");

        // Join the specified room
        await client.invoke("JoinRoom", roomId);
        console.log(`Joined room: ${roomId}`);

        // Register SignalR event handlers
        client.on("MemberJoined", handleUserJoined);
        client.on("MemberLeft", handleUserLeft);
        client.on("ReceiveSignal", handleMessageFromPeer);

        // Xử lý tín hiệu từ SignalR để cập nhật trạng thái thiết bị của remote
        client.on("DeviceStateUpdated", (state, senderId) => {
            const { device, enabled } = JSON.parse(state);

            if (senderId === client.connection.connectionId) {
                // Tín hiệu đến từ chính người dùng (local)
                if (device === "camera") {
                    const localVideoElement = document.getElementById("user-1");
                    if (enabled) {
                        localVideoElement.style.display = "block";
                    } else {
                        localVideoElement.style.display = "none";
                    }
                    console.log(`Local camera ${enabled ? "enabled" : "disabled"}`);
                }
            } else {
                // Tín hiệu đến từ remote user
                if (device === "camera") {
                    const remoteVideoElement = document.getElementById("user-2");
                    if (enabled) {
                        remoteVideoElement.style.display = "block";
                    } else {
                        remoteVideoElement.style.display = "none";
                    }
                    console.log(`Remote camera ${enabled ? "enabled" : "disabled"}`);
                }
            }
        });

        //client.on("CallAccepted", () => {
        //    if (!holdMusic.paused) {
        //        holdMusic.pause();
        //        holdMusic.currentTime = 0; // Reset thời gian phát về đầu
        //        console.log("Hold music stopped");
        //    }
        //});

        //client.on("CallEnded", () => {
        //    if (!holdMusic.paused) {
        //        holdMusic.pause();
        //        holdMusic.currentTime = 0; // Reset thời gian phát về đầu
        //        console.log("Hold music stopped");
        //    }
        //});

        // Initialize local video stream
        localStream = await navigator.mediaDevices.getUserMedia(constraints);
        const localVideoElement = document.getElementById("user-1");
        localVideoElement.srcObject = localStream;
        console.log("Local video stream initialized");
    } catch (error) {
        3
        console.error("Error initializing:", error);
    }
};

// Handle a new user joining the room
const handleUserJoined = async (memberId) => {
    console.log("User joined:", memberId);

    //nhạc chờ
    //if (holdMusic.paused) {
    //    holdMusic.play().catch(err => console.error("Unable to play music:", err));
    //    console.log("Playing hold music");
    //}

    await createPeerConnection(memberId);
    await createOffer(memberId);
};

// Handle a user leaving the room
const handleUserLeft = (memberId) => {
    console.log("User left:", memberId);

    const remoteVideoElement = document.getElementById("user-2");
    remoteVideoElement.srcObject = null;
    remoteVideoElement.style.display = "none";

    const localVideoElement = document.getElementById("user-1");
    localVideoElement.classList.remove("smallFrame");

    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
        console.log("PeerConnection closed");
    }
};

// Handle messages (offer/answer/candidate) from peers
const handleMessageFromPeer = async (signal, memberId) => {
    const message = JSON.parse(signal);

    try {
        if (message.type === "offer") {
            console.log("Offer received:", message.offer);
            await createPeerConnection(memberId);
            await createAnswer(memberId, message.offer);
        } else if (message.type === "answer") {
            console.log("Answer received:", message.answer);
            await addAnswer(message.answer);
        } else if (message.type === "candidate") {
            console.log("Candidate received:", message.candidate);
            if (peerConnection) {
                await peerConnection.addIceCandidate(message.candidate);
            }
        }
    } catch (error) {
        console.error("Error handling peer message:", error);
    }
};

// Create PeerConnection
const createPeerConnection = async (memberId) => {
    try {
        if (!localStream) {
            console.log("Initializing localStream...");
            localStream = await navigator.mediaDevices.getUserMedia(constraints);
        }

        peerConnection = new RTCPeerConnection(servers);
        console.log("RTCPeerConnection created");

        remoteStream = new MediaStream();
        const remoteVideoElement = document.getElementById("user-2");
        remoteVideoElement.srcObject = remoteStream;
        remoteVideoElement.style.display = "block";

        document.getElementById("user-1").classList.add("smallFrame");

        localStream.getTracks().forEach((track) => {
            peerConnection.addTrack(track, localStream);
            console.log(`Local track added: ${track.kind}`);
        });

        peerConnection.ontrack = (event) => {
            console.log("Remote track received:", event);
            event.streams[0].getTracks().forEach((track) => {
                remoteStream.addTrack(track);
                console.log(`Remote track added: ${track.kind}`);
            });
        };

        peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                console.log("Sending ICE candidate:", event.candidate);
                client.invoke("SendSignal", JSON.stringify({ type: "candidate", candidate: event.candidate }), memberId);
            }
        };

        peerConnection.oniceconnectionstatechange = () => {
            console.log("ICE Connection State:", peerConnection.iceConnectionState);
        };
    } catch (error) {
        console.error("Error creating peer connection:", error);
    }
};

// Create offer
const createOffer = async (memberId) => {
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);

    client.invoke("SendSignal", JSON.stringify({ type: "offer", offer }), memberId);
};

// Create answer
const createAnswer = async (memberId, offer) => {
    await peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);

    client.invoke("SendSignal", JSON.stringify({ type: "answer", answer }), memberId);
};

// Add answer to peer connection
const addAnswer = async (answer) => {
    if (!peerConnection.currentRemoteDescription) {
        await peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    }
};

// Toggle camera
/*const toggleCamera = async () => {
    const videoTrack = localStream.getTracks().find((track) => track.kind === "video");

    if (videoTrack) {
        videoTrack.enabled = !videoTrack.enabled;
        document.getElementById("camera-btn").style.backgroundColor = videoTrack.enabled ? "#4a5df5" : "rgb(255, 80, 80)";
        console.log(`Camera ${videoTrack.enabled ? "enabled" : "disabled"}`);
    }
};

// Toggle microphone
const toggleMic = async () => {
    const audioTrack = localStream.getTracks().find((track) => track.kind === "audio");

    if (audioTrack) {
        audioTrack.enabled = !audioTrack.enabled;
        document.getElementById("mic-btn").style.backgroundColor = audioTrack.enabled ? "#4a5df5" : "rgb(255, 80, 80)";
        console.log(`Microphone ${audioTrack.enabled ? "enabled" : "disabled"}`);
    }
};*/

// Toggle camera
const toggleCamera = async () => {
    const videoTrack = localStream.getTracks().find((track) => track.kind === "video");

    if (videoTrack) {
        videoTrack.enabled = !videoTrack.enabled;

        // Cập nhật giao diện local
        const localVideoElement = document.getElementById("user-1");
        if (videoTrack.enabled) {
            localVideoElement.style.display = "block";
            document.getElementById("camera-btn").style.backgroundColor = "#4a5df5"; // Enabled
        } else {
            localVideoElement.style.display = "none";
            document.getElementById("camera-btn").style.backgroundColor = "rgb(255, 80, 80)"; // Disabled
        }
        console.log(`Camera ${videoTrack.enabled ? "enabled" : "disabled"}`);

        // Gửi tín hiệu trạng thái camera qua SignalR
        await client.invoke("UpdateDeviceState", JSON.stringify({
            device: "camera",
            enabled: videoTrack.enabled
        }));
    }
};

// Toggle microphone
/*const toggleMic = async () => {
    const audioTrack = localStream.getTracks().find((track) => track.kind === "audio");

    if (audioTrack) {
        audioTrack.enabled = !audioTrack.enabled;

        // Thay đổi giao diện local
        document.getElementById("mic-btn").style.backgroundColor = audioTrack.enabled ? "#4a5df5" : "rgb(255, 80, 80)";
        console.log(`Microphone ${audioTrack.enabled ? "enabled" : "disabled"}`);

        // Gửi tín hiệu cập nhật trạng thái mic qua SignalR
        await client.invoke("UpdateDeviceState", JSON.stringify({
            device: "mic",
            enabled: audioTrack.enabled
        }));
    }
};*/

const toggleMic = async () => {
    const audioTrack = localStream.getTracks().find((track) => track.kind === "audio");

    if (audioTrack) {
        audioTrack.enabled = !audioTrack.enabled;

        // Cập nhật giao diện local
        document.getElementById("mic-btn").style.backgroundColor = audioTrack.enabled ? "#4a5df5" : "rgb(255, 80, 80)";
        console.log(`Microphone ${audioTrack.enabled ? "enabled" : "disabled"}`);

        // Gửi tín hiệu trạng thái mic qua SignalR
        await client.invoke("UpdateDeviceState", JSON.stringify({
            device: "mic",
            enabled: audioTrack.enabled
        }));
    }
};


// Event listeners
document.getElementById("camera-btn").addEventListener("click", toggleCamera);
document.getElementById("mic-btn").addEventListener("click", toggleMic);

// Cleanup on unload
window.addEventListener("beforeunload", async () => {
    if (client) {
        await client.invoke("LeaveRoom", roomId);
        await client.stop();
    }
});

// Initialize
init();