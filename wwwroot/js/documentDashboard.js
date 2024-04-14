function sendQuery() {
    const query = document.getElementById('queryInput').value;
    const vectorstore = sessionStorage.getItem("sessionNamespace");
    const type = "chat";
    appendChatBubble(query, 'user');
    document.getElementById('queryInput').value = "";
    ws.send(JSON.stringify({ query,vectorstore,type }));
    // document.getElementById('send-button').disabled = true; 
    var chatWindow = document.getElementById('chat-window');
    chatWindow.scrollTop = chatWindow.scrollHeight;
}

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