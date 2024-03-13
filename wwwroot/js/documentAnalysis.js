
function displayError(errorMessage)
{
    var errorBackground = document.getElementById("error-background");
    var errorCloseButton = document.getElementById("error-close");
    var errorText = document.getElementById("error-message");
    errorText.innerText = errorMessage;
    errorBackground.style.display = "block";
    errorCloseButton.onclick = function ()
    {
        errorBackground.style.display = "none";
    }
}
function showLoader()
{
    document.getElementById("loader-spinner").style.display = "";
    document.getElementById("page-container").style.opacity = 0.5;
}

function hideLoader()
{
    document.getElementById("loader-spinner").style.display = "none";
    document.getElementById("page-container").style.opacity = 1;

}
function loadSummary()
{
    document.getElementById("summary-box").innerText = localStorage.getItem("documentSummary");
}


function openWebSocket() {
    ws.onopen = () =>
    {
        console.log('WebSocket connection established');
    };

    ws.onmessage = (event) =>
    {
        const data = JSON.parse(event.data);
        if (data.response)
        {
            var responseText = document.createElement('p');
            responseText.innerText = data.response.response
            document.getElementById('response-output').appendChild(responseText)
        } else if (data.error)
        {
            var responseText = document.createElement('p');
            responseText.innerText = data.error
            document.getElementById('response-output').appendChild(responseText)
        }
    };

    function sendQuery()
    {
        let ws = new WebSocket('ws://localhost:3000');

        const query = document.getElementById('queryInput').value;
        const vectorstore = 'TestSuite';
        ws.send(JSON.stringify({ query,vectorstore }));
    }
}



window.onload = function ()
{
    //let ws = new WebSocket('ws://localhost:3000');
    //openWebSocket();
    document.addEventListener("DOMContentLoaded",function ()
    {
        loadSummary();
    });
    hideLoader();
};
