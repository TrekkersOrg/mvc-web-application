

function appendChatBubble(message, sender) {
    var chatContainer = document.getElementById('chat-window');
    var bubble = document.createElement('div');
    bubble.classList.add('chatbot-query-container');
    var text = document.createElement('p');
    text.innerText = message;
    bubble.appendChild(text);
    chatContainer.appendChild(bubble);
    chatContainer.scrollTop = chatContainer.scrollHeight;
}



///