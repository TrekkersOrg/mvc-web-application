async function deleteSelectedFile() {
    const selectedFileName = document.getElementById('selected-file-name');
    const deleteFileButton = document.getElementById("delete-button"); 
    deleteFileButton.addEventListener("click", deleteSelectedFile);
    const requestBody = {
        FileName: sessionStorage.getItem("selectedFiles")
    };
    await fetch(window.location.protocol + "//" + window.location.host + "/api/fileupload/delete",{
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(requestBody)
    })
        .then(response =>
        {
            if (!response.ok)
            {
                displayError("System is under maintenance. Please try again later.")
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
        })
        .then(data =>
        {

            const uploadFileDisplay = document.getElementById('uploadFileDisplayPanel');
            if (uploadFileDisplay.classList.contains('d-none')) {
                // Keep uploadFileDisplay hidden
            } else {
                uploadFileDisplay.classList.add('d-none');
            }
        })
        .catch(error =>
        {
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
    sessionStorage.removeItem("selectedFiles",selectedFileName);
    
}

async function uploadDocumentToApplication()
{
    const uploadButton = document.getElementById('upload-button');
    const selectedFileName = document.getElementById('selected-file-name');
    const fileInput = document.createElement("input");
    fileInput.type = "file";

    fileInput.addEventListener("change",(event) =>
    {
        const selectedFile = event.target.files[0];
        // Check for supported file types
        const allowedExtensions = ['pdf', 'docx', 'doc'];
        const fileExtension = selectedFile.name.split('.').pop().toLowerCase();
        const deleteButton = document.getElementById('delete-button');
        const uploadFileDisplay = document.getElementById('uploadFileDisplayPanel');

        if (!allowedExtensions.includes(fileExtension)) {
            displayError("Only PDF, DOCX, DOC files are allowed.");
            return;
        }
        
        if (selectedFile)
        {
            const getFileRequestBody = {
                FileName: selectedFile.name
            }
            fetch(window.location.protocol + "//" + window.location.host + "/api/fileupload/getFile", {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(getFileRequestBody)
            })
                .then(response =>
                {
                    if (!response.ok)
                    {
                        displayError("System is under maintenance. Please try again later.");
                    }
                    return response.json();
                })
                .then(data =>
                {
                    const fileExists = data.data.fileExists;
                    selectedFileName.textContent = selectedFile.name;
                    if (fileExists)
                    {
                        displayError("File is already uploaded, please try another file.");
                        return;
                    }
                    else
                    {


                        // Send the selected file to the API for server-side execution
                        const formData = new FormData();
                        formData.append('targetFile',selectedFile);
                        showLoader();
                        fetch('/api/fileupload/upload',{
                            method: 'POST',
                            body: formData
                        })
                            .then(response =>
                            {
                                hideLoader();
                                if (!response.ok)
                                {
                                    displayError("System is under maintenance. Please try again later.")
                                    throw new Error(`HTTP error! Status: ${response.status}`);
                                }
                                return response.json();
                            })
                            .then(data =>
                            {
                                sessionStorage.setItem("selectedFiles",data.data.fileName);
                                if (sessionStorage.getItem("selectedFiles") !== null)
                                {
                                    uploadFileDisplay.classList.remove('d-none');
                                    document.getElementById("next-button").removeAttribute("disabled");
                                    // Show delete button upon file selection
                                    deleteButton.classList.remove('d-none');
                                }
                            })
                            .catch(error =>
                            {
                                hideLoader();
                                displayError("System is under maintenance. Please try again later.")
                                console.log('Upload error:',error);
                            });
                        hideLoader();
                    }
                })
                .catch(error =>
                {
                    displayError("System is under maintenance. Please try again later.")
                    console.log('Get error:',error);
                });
        }
        
    });
    fileInput.click(); // Trigger the file selection dialog
}

async function uploadDocumentToPinecone()
{
    const requestBody = {
        Namespace: sessionStorage.getItem("sessionNamespace"),
        FileName: sessionStorage.getItem("selectedFiles")
    };
    showLoader();
    await fetch(window.location.protocol + "//" + window.location.host + "/api/indexer/insertDocument",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        },
        body: JSON.stringify(requestBody)
    })
        .then(response =>
        {
            hideLoader();
            if (!response.ok)
            {
                sessionStorage.setItem("insertDocumentStatus","fail");
                displayError("System is under maintenance. Please try again later.")
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            sessionStorage.setItem("insertDocumentStatus","success");
            return response.json();
        })
        .then(data =>
        {
            hideLoader();
            sessionStorage.setItem("insertDocumentStatus","success");
        })
        .catch(error =>
        {
            sessionStorage.setItem("insertDocumentStatus","fail");
            hideLoader();
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
    setTimeout(function ()
    {
        console.log("Idle for 10 seconds");
    },10000);
    return Promise.resolve();
}

async function verifyPineconeNamespace()
{
    showLoader();
    await fetch(window.location.protocol + "//" + window.location.host + "/api/indexer/PineconeDetails",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
        .then(response =>
        {
            hideLoader();
            if (!response.ok)
            {
                sessionStorage.setItem("pineconeDetailsStatus","fail");
                displayError("System is under maintenance. Please try again later.")
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            sessionStorage.setItem("pineconeDetailsStatus","success");
            return response.json();
        })
        .then(data =>
        {
            hideLoader();
            sessionStorage.setItem("pineconeDetailsStatus","success");
            var namespaceExistence = data.data && data.data.namespaces && data.data.namespaces[sessionStorage.getItem("sessionNamespace")];
            return namespaceExistence;
        })
        .catch(error =>
        {
            sessionStorage.setItem("pineconeDetailsStatus","fail");
            hideLoader();
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
    return Promise.resolve();
}

function displayError(errorMessage)
{
    var errorBackground = document.getElementById("error-background");
    var errorCloseButton = document.getElementById("error-close");
    var errorText = document.getElementById("error-message");
    errorText.innerText = errorMessage;
    errorBackground.style.display = "block";
    errorCloseButton.onclick = function()
    {
        errorBackground.style.display = "none";
    }
}

function showLoader()
{
    //document.getElementById("loader-spinner").style.display = "";
    document.getElementById("page-container").style.opacity = 0.5;
}

function hideLoader()
{
    //document.getElementById("loader-spinner").style.display = "none";
    document.getElementById("page-container").style.opacity = 1;

}

// Generate document title

async function generateTitle()
{
    const requestBody = {
        Vectorstore: sessionStorage.getItem("sessionNamespace"),
        Query: "Generate a title for this document. The title should be less than 5 words but should detail what the document is."
    };
    showLoader();
    await fetch(window.location.protocol + "//" + window.location.host + "/api/chatbot/sendQuery",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
    })
        .then(response =>
        {
            if (!response.ok)
            {
                sessionStorage.setItem("generateTitleStatus","fail");
                displayError("System is under maintenance. Please try again later.")
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            sessionStorage.setItem("generateTitleStatus","success");

            return response.json();
        })
        .then(data =>
        {
            sessionStorage.setItem("generateTitleStatus","success");
            sessionStorage.setItem("documentTitle",data["data"]["response"]);
            hideLoader();
        })
        .catch(error =>
        {
            sessionStorage.setItem("generateTitleStatus","fail");
            hideLoader();
            displayError("System is under maintenance. Please try again later.");
            console.error('Fetch error:',error);
        });
    return Promise.resolve();
}

function routeToDocumentAnalysis()
{
    var documentAnalysisUrl = window.location.protocol + "//" + window.location.host + '/Home/DocumentDashboard';
    window.location.href = documentAnalysisUrl;
    return Promise.resolve();
}

async function uploadFileFlow()
{
    await uploadDocumentToPinecone();
    if (sessionStorage.getItem("insertDocumentStatus") == "fail")
    {
        return;
    }
    else
    {
        await verifyPineconeNamespace();
        if (sessionStorage.getItem("pineconeDetailsStatus") == "fail")
        {
            return;
        }
        else
        {
            await routeToDocumentAnalysis();
        }
    }
}
window.onload = function ()
{
    hideLoader();
};
