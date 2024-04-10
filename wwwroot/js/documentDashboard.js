function submitChat(event) {
    if (event.keyCode === 13) {
        document.getElementById("send-button").click();
    }
}

function sendQuery() {
    const query = document.getElementById('queryInput').value;
    const type = "chat";
    
    generateUserBubble(document.getElementById('queryInput').value);
    document.getElementById('queryInput').value = "";
    document.getElementById('send-button').disabled = true;
    ws.send(JSON.stringify({ query, vectorstore, type }));
    window.scrollTop = window.scrollHeight;
}


function generateUserBubble(message) {
    var window = document.getElementById('chat-window');
    var container = document.getElementById('chatbot-query-container');
    var bubble = document.createElement('div');

    bubble.classList.add('chat-output');
    var text = document.createElement('p');
    text.innerText = message;
    bubble.appendChild(text);
    container.appendChild(bubble);
    addChatbotLoader();
    window.scrollTop = window.scrollHeight;
}