function submitChat(event) {
    if (event.keyCode === 13) {
        document.getElementById("send-button").click();
    }
}

function sendQuery() {
    const query = document.getElementById('queryInput').value;
    const type = "chat";

    appendChatBubble(query, 'user');

    document.getElementById('queryInput').value = "";
    document.getElementById('send-button').disabled = true;

    var chatWindow = document.getElementById('chat-window');
    chatWindow.scrollTop = chatWindow.scrollHeight;
}

function appendChatBubble(message, sender) {
    var chatContainer = document.getElementById('chat-window');
    var bubble = document.createElement('div');
    bubble.classList.add('chat-output');

    if (sender === 'user'){
        bubble.classList.add('darker');
    }

    var text = document.createElement('p');
    text.innerText = message;
    bubble.appendChild(text);
    chatContainer.appendChild(bubble);

    chatContainer.scrollTop = chatContainer.scrollHeight;
}


///