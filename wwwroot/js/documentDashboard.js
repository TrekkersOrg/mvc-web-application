﻿
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
    var window = document.getElementById('response-container');
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
    var window = document.getElementById('response-container');
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
    sessionNamespace = sessionStorage.getItem('sessionNamespace');
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
    var window = document.getElementById('response-container');
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

async function systemQuery()
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
        body: JSON.stringify({
            namespace: sessionNamespace,
            file_name: sessionStorage.getItem('selectedFile')
        })
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
}

async function generateSummary()
{
    try
    {
        sessionNamespace = sessionStorage.getItem('sessionNamespace');
        query = "Generate 4 short sentence as a brief summary on the document. Include what the document is about."
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
            var summaryResponse = data["data"]["response"];
            document.getElementById('injected-summary').innerText = summaryResponse;
            
        } else
        {
            displayError("ERROR GENERATING SUMMARY");
        }
    } catch (error)
    {
        displayError("ERROR GENERATING SUMMARY");
    }
}

function calculateAverage(scores)
{
    const scoreValues = Object.values(scores);
    const total = scoreValues.reduce((sum,score) => sum + score,0);
    const average = total / scoreValues.length;
    const percentage = (average / 5) * 100;
    return `${percentage.toFixed(2)}%`;
}


function createCircularRiskMeter(percentage) {
    var ctx = document.getElementById('circularRiskMeter').getContext('2d');
    var data = {
        datasets: [{
            data: [percentage, 100 - percentage],
            backgroundColor: ['#003f5c', '#d9d9d9'],
            borderWidth: 0
        }]
    };

    var options = {
        cutout: '80%',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            tooltip: {
                enabled: false
            },
            legend: {
                display: false
            }
        },
        animation: {
            onComplete: function(animation) {
                var chartInstance = animation.chart;
                if (!chartInstance) {
                    console.error("Chart instance is undefined.");
                    return;
                }
                var ctx = chartInstance.ctx;
                if (!ctx) {
                    console.error("Chart context is undefined.");
                    return;
                }

                requestAnimationFrame(() => {
                    ctx.font = 'bold 2em Arial'; // Smaller font size to fit within the doughnut hole
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.fillStyle = 'black'; // Adjust the color as needed

                    var text = Math.round(percentage) + "%", // Convert to whole number
                        centerX = (chartInstance.chartArea.left + chartInstance.chartArea.right) / 2,
                        centerY = (chartInstance.chartArea.top + chartInstance.chartArea.bottom) / 2;

                    ctx.fillText(text, centerX, centerY);
                });
            }
        },
        events: [] // Disable hover events to prevent redrawing
    };

    try {
        new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: options
        });
    } catch (error) {
        console.error("Error creating chart: ", error.message);
    }
}



async function determineRiskScore()
{
    await systemQuery();
    await customRiskAssessment();
    var custom = JSON.parse(sessionStorage.getItem('custom')).data;
    var keywords = JSON.parse(sessionStorage.getItem('keywords')).data;
    var query = JSON.parse(sessionStorage.getItem('query')).data;
    const model1Average = Math.round((query.operationalScore + query.regulatoryScore + query.reputationalScore + query.financialScore) / 4);
    const model2Average = Math.round((keywords.operationalScore + keywords.regulatoryScore + keywords.reputationalScore + keywords.financialScore) / 4);
    const model3Average = Math.round((custom.operationalScore + custom.regulatoryScore + custom.reputationalScore + custom.financialScore) / 4);
    document.getElementById('system-query').parentElement.setAttribute('data-score', model1Average);
    document.getElementById('keywords').parentElement.setAttribute('data-score', model2Average);
    document.getElementById('custom').parentElement.setAttribute('data-score', model3Average);
    document.getElementById('system-query').style.width = `${model1Average * 10}%`;
    document.getElementById('keywords').style.width = `${model2Average * 10}%`;
    document.getElementById('custom').style.width = `${model3Average * 10}%`;
    document.getElementById('system-query').style.width = calculateAverage(query);
    document.getElementById('keywords').style.width = calculateAverage(keywords);
    document.getElementById('custom').style.width = calculateAverage(custom);
    var financialScoreAvg = Math.round((custom.financialScore + keywords.financialScore + query.financialScore) / 3);
    var operationalScoreAvg = Math.round((custom.operationalScore + keywords.operationalScore + query.operationalScore) / 3);
    var regulatoryScoreAvg = Math.round((custom.regulatoryScore + keywords.regulatoryScore + query.regulatoryScore) / 3);
    var reputationalScoreAvg = Math.round((custom.reputationalScore + keywords.reputationalScore + query.reputationalScore) / 3);
    var finalScore = Math.round((operationalScoreAvg + regulatoryScoreAvg) / 2);
    createCircularRiskMeter((finalScore / 5) * 100);
    var averagedScores = JSON.stringify({
        financialScore: financialScoreAvg,
        operationalScore: operationalScoreAvg,
        regulatoryScore: regulatoryScoreAvg,
        reputationalScore: reputationalScoreAvg,
        finalScore: finalScore
    });
    sessionStorage.setItem('riskAssessment',averagedScores);
    const riskAssessmentBody = {
        "namespace": sessionStorage.getItem('sessionNamespace'),
        "file_name": sessionStorage.getItem('selectedFile'),
        "riskAssessmentScore": finalScore,
        "financialScore": financialScoreAvg,
        "financialSystemQueryScore": query.financialScore,
        "financialKeywordsScore": keywords.financialScore,
        "financialXgbScore": custom.financialScore,
        "reputationalScore": reputationalScoreAvg,
        "reputationalSystemQueryScore": query.reputationalScore,
        "reputationalKeywordsScore": keywords.reputationalScore,
        "reputationalXgbScore": custom.reputationalScore,
        "regulatoryScore": regulatoryScoreAvg,
        "regulatorySystemQueryScore": query.regulatoryScore,
        "regulatoryKeywordsScore": keywords.regulatoryScore,
        "regulatoryXgbScore": custom.regulatoryScore,
        "operationalScore": operationalScoreAvg,
        "operationalSystemQueryScore": query.operationalScore,
        "operationalKeywordsScore": keywords.operationalScore,
        "operationalXgbScore": custom.operationalScore
    };
    try
    {
        const response = await fetch(`https://strive-api.azurewebsites.net/api/mongodb/addRiskAssessment`,{
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(riskAssessmentBody)
        });
    } catch (error)
    {
        displayError('error');
        return;
    }
}

async function viewDocument()
{
    try
    {
        var namespace = sessionStorage.getItem('sessionNamespace');
        var fileName = sessionStorage.getItem('selectedFile');
        var encodedFileName = encodeURIComponent(fileName);
        var version = 0;
        const url = `https://strive-api.azurewebsites.net/api/mongodb/getdocumentcontent?collectionName=${namespace}&fileName=${encodedFileName}&version=${version}`;
        const response = await fetch(url,{
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        var documentData = await response.json();
        var content = documentData.data.content;
        const openDocument = window.open();
        openDocument.document.write(`
            <html>
                <body>
                    <h2>Document Name: ${fileName}</h2>
                    <h2>Version: ${version}</h2>
                    <p>${content}</p>
                </body>
            </html>
        `);
        openDocument.document.close();
    } catch (error)
    {
        displayError('error');
        return;
    }
}

async function downloadExcel()
{
    try
    {
        var namespace = sessionStorage.getItem('sessionNamespace');
        var fileName = sessionStorage.getItem('selectedFile');
        var version = 0;
        const url = `https://strive-api.azurewebsites.net/api/mongodb/getdocument?collectionName=${namespace}&fileName=${fileName}&version=${version}`;
        const response = await fetch(url,{
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        var documentData = await response.json();
    } catch (error)
    {
        displayError('error');
        return;
    }
    const riskAssessmentScores = {
        risk_assessment: {
            score: documentData.data.riskAssessmentScore,
            financial: {
                score: documentData.data.financialScore,
                system_query: documentData.data.financialSystemQueryScore,
                keywords: documentData.data.financialKeywordsScore,
                xgb: documentData.data.financialXgbScore
            },
            reputational: {
                score: documentData.data.reputationalScore,
                system_query: documentData.data.reputationalSystemQueryScore,
                keywords: documentData.data.reputationalKeywordsScore,
                xgb: documentData.data.reputationalXgbScore
            },
            regulatory: {
                score: documentData.data.regulatoryScore,
                system_query: documentData.data.regulatorySystemQueryScore,
                keywords: documentData.data.regulatoryKeywordsScore,
                xgb: documentData.data.regulatoryXgbScore
            },
            operational: {
                score: documentData.data.operationalScore,
                system_query: documentData.data.operationalSystemQueryScore,
                keywords: documentData.data.operationalKeywordsScore,
                xgb: documentData.data.operationalXgbScore
            }
        }
    };
    const data = [
        ["Document Name",sessionStorage.getItem("selectedFile"),"","",""],
        ["Date",new Date().toLocaleString(),"","",""],
        ["Risk Assessment Score",riskAssessmentScores.risk_assessment.score,"","",""],
        ["","","","",""],
        ["","Model 1","Model 2","Model 3","Weighted Score"],
        ["Operational",riskAssessmentScores.risk_assessment.operational.system_query,riskAssessmentScores.risk_assessment.operational.keywords,riskAssessmentScores.risk_assessment.operational.xgb,riskAssessmentScores.risk_assessment.operational.score],
        ["Compliance",riskAssessmentScores.risk_assessment.regulatory.system_query,riskAssessmentScores.risk_assessment.regulatory.keywords,riskAssessmentScores.risk_assessment.regulatory.xgb,riskAssessmentScores.risk_assessment.regulatory.score],
        ["Reputational",riskAssessmentScores.risk_assessment.reputational.system_query,riskAssessmentScores.risk_assessment.reputational.keywords,riskAssessmentScores.risk_assessment.reputational.xgb,riskAssessmentScores.risk_assessment.reputational.score],
        ["Financial",riskAssessmentScores.risk_assessment.financial.system_query,riskAssessmentScores.risk_assessment.financial.keywords,riskAssessmentScores.risk_assessment.financial.xgb,riskAssessmentScores.risk_assessment.financial.score]
    ];
    const worksheet = XLSX.utils.aoa_to_sheet(data);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook,worksheet,'Risk Assessment');
    const wbout = XLSX.write(workbook,{ bookType: 'xlsx',type: 'array' });
    const blob = new Blob([wbout],{ type: 'application/octet-stream' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'RiskAssessment- ' + new Date().toISOString() + '.xlsx';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.onload = async function ()
{
    showLoader();
    sessionStorage.setItem("conversationMemory",JSON.stringify([]));
    document.getElementById('queryInput').addEventListener('input',checkInput);
    var documentData = JSON.parse(sessionStorage.getItem('documentContext'));
    var documentName = '';
    if (documentData['documentName'])
    {
        documentName = documentData['documentName'];
    }
    else
    {
        documentName = sessionStorage.getItem('selectedFile');
    }
    document.getElementById('document-title').innerText = documentName;
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
                    count: 6,
                    font: {
                        size: 16 // Increase tick font size
                    },
                },
                pointLabels: {
                    font: {
                        size: 18 // Increase label font size
                }
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

}
function openNav() {
    document.getElementById("sidebar").style.width = "250px";
    document.getElementById("main-content").style.marginLeft = "250px";
    document.querySelector('.chat-container').style.width = "30vw"; // Adjust the width of the chatbot
    document.querySelector('.chart-col').style.width = "50%"; // Adjust the width of the risk chart
    document.querySelector('#riskMeter').style.width = "90%"; // Adjust the width of the risk meter
}

function closeNav() {
    document.getElementById("sidebar").style.width = "0";
    document.getElementById("main-content").style.marginLeft = "0";
    document.querySelector('.chat-container').style.width = "40vw"; // Revert to the original width of the chatbot
    document.querySelector('.chart-col').style.width = "60%"; // Revert to the original width of the risk chart
    document.querySelector('#riskMeter').style.width = "100%"; // Revert to the original width of the risk meter
}

function toggleIconColor() {
    var svgIcon = document.querySelector('.view-file-icon');
    if (svgIcon.classList.contains('active')) {
        svgIcon.classList.remove('active');
    } else {
        svgIcon.classList.add('active');
    }
}

document.querySelectorAll('.small-risk-meter').forEach(function(meter) {
    meter.addEventListener('mouseenter', function() {
        const score = this.getAttribute('data-score');  // Get the score from the data attribute

        // Create tooltip to show the score
        const tooltip = document.createElement('div');
        tooltip.className = 'hover-tooltip';
        tooltip.innerText = `${score}/10`;  // Show the score out of 10

        this.appendChild(tooltip);
    });
    
    meter.addEventListener('mouseleave', function() {
        const tooltip = this.querySelector('.hover-tooltip');
        if (tooltip) {
            tooltip.remove();
        }
    });
});

