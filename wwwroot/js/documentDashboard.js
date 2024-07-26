
var generatingResponse = false;

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

function checkInput()
{
    var input = document.getElementById('queryInput').value.trim();
    var sendButton = document.getElementById('send-button');
    if (input !== '' && generatingResponse == false)
    {
        sendButton.disabled = false;
    } else
    {
        sendButton.disabled = true;
    }
}

function submitChat(event)
{
    if (event.keyCode === 13)
    {
        document.getElementById("send-button").click();
    }
}

function removeSubsequentBubbles(bubbleId)
{
    const bubbles = document.querySelectorAll('.chat-output');
    let targetQueryIndex = Array.from(bubbles).indexOf(document.getElementById(bubbleId));
    for (let i = 0; i < bubbles.length; i++)
    {
        if (i >= targetQueryIndex)
        {
            bubbles[i].remove();
        }
    }
}

async function generateUserBubble(message)
{
    checkInput();
    var window = document.getElementById('chat-window');
    var bubble = document.createElement('div');
    bubble.id = Date.now();
    bubble.classList.add('chat-output');
    bubble.classList.add('darker');
    var text = document.createElement('p');
    text.classList.add('bubble-text')
    text.innerText = message;
    bubble.appendChild(text);
    var editButton = document.createElement('button');
    editButton.innerText = '✎';
    editButton.classList.add('edit-button');
    var saveButton = document.createElement('button');
    saveButton.innerHTML = '✔';
    saveButton.classList.add('save-button');
    var cancelButton = document.createElement('button');
    cancelButton.innerHTML = '✖';
    cancelButton.classList.add('cancel-button');
    var actionButtonContainer = document.createElement('div');
    actionButtonContainer.appendChild(editButton);
    actionButtonContainer.appendChild(cancelButton);
    actionButtonContainer.appendChild(saveButton);
    bubble.appendChild(actionButtonContainer);
    window.appendChild(bubble);
    window.scrollTop = window.scrollHeight;
    editButton.addEventListener('click',function ()
    {
        text.contentEditable = 'true';
        text.classList.add('editable');
        editButton.style.display = 'none';
        saveButton.style.display = 'inline';
        cancelButton.style.display = 'inline';
    });
    saveButton.addEventListener('click',async function ()
    {
        text.contentEditable = 'false';
        text.classList.remove('editable');
        editButton.style.display = 'inline';
        saveButton.style.display = 'none';
        cancelButton.style.display = 'none';
        removeSubsequentBubbles(bubble.id);
        var sessionNamespace = '';
        var context = '';
        if (sessionStorage.getItem('documentContext'))
        {
            context = JSON.parse(sessionStorage.getItem('documentContext'))?.documentDescription;
        }
        if (!sessionStorage.getItem('sessionNamespace'))
        {
            sessionNamespace = 'TestSuite';
        }
        else
        {
            sessionNamespace = sessionStorage.getItem('sessionNamespace');
        }
        generatingResponse = true;
        var query = text.innerText;
        var baseQuery = query;
        if (context != '')
        {
            query = "Use this as context for your response: " + context + ". Answer the following query: " + query;
        }
        document.getElementById('queryInput').value = "";
        await generateUserBubble(baseQuery);
        try
        {
            var sendButton = document.getElementById('send-button');
            sendButton.disabled = true;
            const response = await fetch('/api/striveml/chatbot',{
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    namespace: sessionNamespace,
                    query: query,
                    file_name: sessionStorage.getItem('selectedFile')
                })
            });

            var data = await response.json();
            if (response.ok)
            {
                generateSystemBubble(data["data"]["response"]);
                generatingResponse = false;
                var newConversation = {
                    "newQuery": query,
                    "newResponse": data["data"]["response"]
                };
                sessionStorage.setItem("newConversation",JSON.stringify(newConversation));
                conversationMemoryEntry();
                checkInput();
            } else
            {
                generateSystemBubble('ERROR GENERATING RESPONSE');
                generatingResponse = false;
                checkInput();
            }
        } catch (error)
        {
            console.error('Error: ',error.message);
            generateSystemBubble('ERROR GENERATING RESPONSE');
            generatingResponse = false;
            checkInput();
        }
    });
    cancelButton.addEventListener('click',function ()
    {
        text.contentEditable = 'false';
        text.classList.remove('editable');
        text.innerText = message; // Revert to original text
        editButton.style.display = 'inline';
        saveButton.style.display = 'none';
        cancelButton.style.display = 'none';
    });
    checkInput();
    addChatbotLoader();
}

function generateSystemBubble(message)
{
    checkInput();
    var bubble = document.createElement('div');
    bubble.classList.add('chat-output');
    var text = document.createElement('p');
    text.classList.add('bubble-text');
    bubble.appendChild(text);
    var window = document.getElementById('chat-window');
    removeChatbotLoader();
    window.appendChild(bubble);
    var index = 0;
    var sendButton = document.getElementById('send-button');
    sendButton.disabled = true;
    var typingEffect = setInterval(function ()
    {
        if (index < message.length)
        {
            text.innerText = message.slice(0,index);
            index++;
            sendButton.disabled = true;
        } else
        {
            clearInterval(typingEffect);
            text.innerText = message;
        }
    },10);
    document.getElementById('send-button').disabled = false;
}

async function newUpload()
{
    var sessionNamespace = sessionStorage.getItem('sessionNamespace');
    try
    {
        document.getElementById('upload-button').disabled = true;
        const url = `https://strive-api.azurewebsites.net/api/pinecone/purgePinecone`;
        const pineconeResponse = await fetch(url,{
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                namespace: sessionNamespace
            })
        });

        var data = await pineconeResponse.json();
        if (pineconeResponse.ok)
        {
            var documentAnalysisUrl = `${window.location.protocol}//${window.location.host}/Home/FileUpload`;
            window.location.href = documentAnalysisUrl;
        } else
        {
            displayError('Failed to process file.');
        }
    } catch (error)
    {
        displayError('Failed to process file.');
        document.getElementById('upload-button').disabled = false;
        return;
    }
}

async function sendQuery()
{
    var sessionNamespace = '';
    var context = '';
    if (sessionStorage.getItem('documentContext'))
    {
        context = JSON.parse(sessionStorage.getItem('documentContext'))?.documentDescription;
    }
    if (!sessionStorage.getItem('sessionNamespace'))
    {
        sessionNamespace = 'TestSuite';
    }
    else
    {
        sessionNamespace = sessionStorage.getItem('sessionNamespace');
    }
    generatingResponse = true;
    checkInput();
    var query = document.getElementById('queryInput').value;
    var baseQuery = query;
    if (context != '')
    {
        query = "Use this as context for your response: " + context + ". Answer the following query: " + query;
    }
    document.getElementById('queryInput').value = "";
    await generateUserBubble(baseQuery);
    try
    {
        var sendButton = document.getElementById('send-button');
        sendButton.disabled = true;
        const response = await fetch('/api/striveml/chatbot',{
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                namespace: sessionNamespace,
                query: query,
                file_name: sessionStorage.getItem('selectedFile')
            })
        });

        var data = await response.json();
        if (response.ok)
        {
            generateSystemBubble(data["data"]["response"]);
            generatingResponse = false;
            var newConversation = {
                "newQuery": query,
                "newResponse": data["data"]["response"]
            };
            sessionStorage.setItem("newConversation",JSON.stringify(newConversation));
            conversationMemoryEntry();
            checkInput();
        } else
        {
            generateSystemBubble('ERROR GENERATING RESPONSE');
            generatingResponse = false;
            checkInput();
        }
    } catch (error)
    {
        console.error('Error: ',error.message);
        generateSystemBubble('ERROR GENERATING RESPONSE');
        generatingResponse = false;
        checkInput();
    }
}

function addChatbotLoader()
{
    var parent = document.createElement('div');
    parent.classList.add('chat-output');
    parent.setAttribute('id','chat-loader');
    var bubble = document.createElement('div');
    bubble.classList.add('typing');
    var circle = document.createElement('span');
    circle.classList.add('circle');
    circle.classList.add('scaling');
    bubble.appendChild(circle);
    parent.appendChild(bubble);
    var window = document.getElementById('chat-window');
    window.appendChild(parent);
    checkInput();
}

function removeChatbotLoader()
{
    const loader = document.getElementById('chat-loader');
    loader && loader.remove();
}

function showLoader()
{
    document.getElementById("page-container").style.display = 'block';
}

function hideLoader()
{
    document.getElementById("page-container").style.display = 'none';
}

async function customRiskAssessment()
{
    const fileName = sessionStorage.getItem('selectedFile');
    const namespace = sessionStorage.getItem('sessionNamespace');
    const requestBody = {
        namespace: namespace,
        file_name: fileName
    };
    showLoader();
    await fetch("https://strive-core.azurewebsites.net/xgboostModel",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': '*',
            'Access-Control-Allow-Headers': '*'
        },
        body: JSON.stringify(requestBody)
    })
        .then(response =>
        {
            if (!response.ok)
            {
                hideLoader();
                displayError('Failed to process file.');
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            return response.text();
        })
        .then(data =>
        {
            sessionStorage.setItem('custom',data);
        })
        .catch(error =>
        {
            hideLoader();
            displayError('Failed to process file.');
            console.error('Fetch error:',error);
        });
}

function conversationMemoryEntry()
{
    var conversationMemory = JSON.parse(sessionStorage.getItem("conversationMemory"));
    var newConversation = JSON.parse(sessionStorage.getItem("newConversation"));
    console.log(conversationMemory);
    console.log(conversationMemory.length);
    if (conversationMemory.length == 0)
    {
        var conv = {
            "query1": newConversation["newQuery"],
            "response1": newConversation["newResponse"]
        };
        conversationMemory.push(conv);
        sessionStorage.setItem("conversationMemory",JSON.stringify(conversationMemory));
        return;
    } else if (conversationMemory.length < 5)
    {
        var queryKeyLabel = "query" + (conversationMemory.length + 1);
        var responseKeyLabel = "response" + (conversationMemory.length + 1);
        var conv = {};
        conv[queryKeyLabel] = newConversation["newQuery"];
        conv[responseKeyLabel] = newConversation["newResponse"];
        conversationMemory.push(conv);
        sessionStorage.setItem("conversationMemory",JSON.stringify(conversationMemory));
        return;
    } else if (conversationMemory.length >= 5)
    {
        conversationMemory.shift();
        var newQueryKeyName = "";
        var newResponseKeyName = "";
        conversationMemory.forEach(function (dictionary)
        {
            console.log("Dictionary: " + JSON.stringify(dictionary));
            var keys = Object.keys(dictionary);
            console.log(keys);
            keys.forEach(function (key)
            {
                if (key.includes("query"))
                {
                    var index = parseInt(key[key.length - 1]);
                    index++;
                    newQueryKeyName = "query" + index.toString();
                    dictionary[newQueryKeyName] = dictionary[key];
                    delete dictionary[key];
                }
                if (key.includes("response"))
                {
                    var index = parseInt(key[key.length - 1]);
                    index++;
                    newResponseKeyName = "response" + index.toString();
                    dictionary[newResponseKeyName] = dictionary[key];
                    delete dictionary[key];
                }
            });
        });
        newQueryKeyName = "query" + (conversationMemory.length + 1);
        newResponseKeyName = "response" + (conversationMemory.length + 1);
        var conv = {};
        conv[newQueryKeyName] = newConversation["newQuery"];
        conv[newResponseKeyName] = newConversation["newResponse"];
        conversationMemory.push(conv);
        sessionStorage.setItem("conversationMemory",JSON.stringify(conversationMemory));
        return;
    }
    return;
}

/*async function systemQuery()
{
    const requestBody = {
        namespace: sessionStorage.getItem("sessionNamespace")
    };
    showLoader();
    await fetch("https://strive-core.azurewebsites.net/systemQueryModel",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': '*',
            'Access-Control-Allow-Headers': '*'
        },
        body: {
            namespace: sessionNamespace,
            query: query,
            file_name: sessionStorage.getItem('selectedFile')
        }
    })
        .then(response =>
        {
            if (!response.ok)
            {
                hideLoader();
                displayError('Failed to process file.');
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            return response.text();
        })
        .then(data =>
        {
            sessionStorage.setItem('query',data);
        })
        .catch(error =>
        {
            hideLoader();
            displayError('Failed to process file.');
            console.error('Fetch error:',error);
        });
}*/

async function generateSummary()
{
    try
    {
        sessionNamespace = sessionStorage.getItem('sessionNamespace');
        query = "Generate a 3-7 sentence summary on the document. Include what the document is about."
        const response = await fetch('/api/striveml/chatbot',{
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                namespace: sessionNamespace,
                query: query,
                file_name: sessionStorage.getItem('selectedFile')
            })
        });

        var data = await response.json();
        if (response.ok)
        {
            var summaryResponse = data["data"]["response"] + " Feel free to ask further questions regarding this document.";
            generateSystemBubble(summaryResponse);
        } else
        {
            displayError("ERROR GENERATING SUMMARY");
        }
    } catch (error)
    {
        displayError("ERROR GENERATING SUMMARY");
    }
}

async function determineRiskScore()
{
    //await systemQuery();
    var custom = JSON.parse(sessionStorage.getItem('custom')).data;
    var keywords = JSON.parse(sessionStorage.getItem('keywords')).data;
    var query = JSON.parse(sessionStorage.getItem('keywords')).data;
    var financialScoreAvg = Math.round((custom.financialScore + keywords.financialScore + query.financialScore) / 3);
    var operationalScoreAvg = Math.round((custom.operationalScore + keywords.operationalScore + query.operationalScore) / 3);
    var regulatoryScoreAvg = Math.round((custom.regulatoryScore + keywords.regulatoryScore + query.regulatoryScore) / 3);
    var reputationalScoreAvg = Math.round((custom.reputationalScore + keywords.reputationalScore + query.reputationalScore) / 3);
    var finalScore = Math.round((financialScoreAvg + regulatoryScoreAvg) / 2);
    var averagedScores = JSON.stringify({
        financialScore: financialScoreAvg,
        operationalScore: operationalScoreAvg,
        regulatoryScore: regulatoryScoreAvg,
        reputationalScore: reputationalScoreAvg,
        finalScore: finalScore
    });
    sessionStorage.setItem('riskAssessment',averagedScores);

}

window.onload = async function ()
{
    showLoader();
    sessionStorage.setItem("conversationMemory",JSON.stringify([]));
    document.getElementById('queryInput').addEventListener('input',checkInput);
    checkInput();

    // Chart.js and risk meter initialization
    var ctx = document.getElementById('spiderChart').getContext('2d');
    var spiderChart = new Chart(ctx,{
        type: 'radar',
        data: {
            labels: ['Operational Risk','Financial Risk','Reputational Risk','Compliance Risk'],
            datasets: [{
                label: 'Risk Levels',
                data: [0,0,0,0], // Example data, you can update this with dynamic values
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scale: {
                ticks: {
                    beginAtZero: true,
                    stepSize: 1,
                    count: 6
                },
                r: {
                    suggestedMin: 0,
                    suggestedMax: 5
                }
            },
            plugins: {
                legend: {
                    position: 'top',
                    align: 'start'
                }
            }
        }
    });
    await generateSummary();
    await customRiskAssessment();
    await determineRiskScore();
    hideLoader();
    var riskAssessmentData = JSON.parse(sessionStorage.getItem('riskAssessment'));
    var finalScore = riskAssessmentData.finalScore;
    var financialScore = riskAssessmentData.financialScore;
    var operationalScore = riskAssessmentData.operationalScore;
    var regulatoryScore = riskAssessmentData.regulatoryScore;
    var reputationalScore = riskAssessmentData.reputationalScore;
    spiderChart.data.datasets[0].data = [operationalScore,financialScore,reputationalScore,regulatoryScore];
    spiderChart.update();
    // Risk Meter
    function updateRiskMeter(value)
    {
        var riskMeterFill = document.getElementById('riskMeterFill');
        var riskMeterLabel = document.getElementById('riskMeterLabel');
        var percentage = value * 20; // Percentage of risk meter fill
        riskMeterFill.style.width = percentage + '%';

        // Adjust label value based on percentage
        //var labelValue = percentage < 50 ? percentage / 10 : percentage / 20 + 2.5;
        riskMeterLabel.innerText = value.toFixed(1);
    }

    // Example usage with a random value between 0 and 1
    setInterval(function ()
    {
        //var randomValue = Math.random(); // Random value between 0 and 1
        updateRiskMeter(finalScore);
    },2000); // Update every 2 seconds
}