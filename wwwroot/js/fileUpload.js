const fileInput = document.getElementById('fileInput');
const uploadStatus = document.getElementById('uploadStatus');
const documentNameInput = document.getElementById('documentName');
const documentTypeInput = document.getElementById('documentType');
const documentDescriptionInput = document.getElementById('documentDescription');
const nextButton = document.getElementById('nextButton');

function validateFiles()
{
    const uploadedFiles = JSON.parse(sessionStorage.getItem('uploadedFiles')) || [];
    nextButton.disabled = uploadedFiles.length === 0;
}

function checkNextActivation()
{
    if (sessionStorage.getItem('selectedFile'))
    {
        const documentName = document.getElementById('documentName');
        const documentType = document.getElementById('documentType');
        const documentDescription = document.getElementById('documentDescription');

        const isDocumentNameFilled = documentName && documentName.value.trim();
        const isDocumentTypeFilled = documentType && documentType.value.trim();
        const isDocumentDescriptionFilled = documentDescription && documentDescription.value.trim();

        // Check if all fields are filled or all are empty
        if (
            (isDocumentNameFilled && isDocumentTypeFilled && isDocumentDescriptionFilled) ||
            (!isDocumentNameFilled && !isDocumentTypeFilled && !isDocumentDescriptionFilled)
        )
        {
            console.log('Enabled.');
            nextButton.disabled = false;
        } else
        {
            console.log('Disabled.');
            nextButton.disabled = true;
        }
    } else
    {
        console.log('Disabled.');
        nextButton.disabled = true;
    }
}

async function validateFileExistence()
{
    var namespace = sessionStorage.getItem('sessionNamespace');
    var fileName = sessionStorage.getItem('selectedFile');
    const url = `https://strive-api.azurewebsites.net/api/MongoDB/getdocument?collectionName=${namespace}&fileName=${fileName}&version=0`;

    const response = await fetch(url,{
        method: 'GET'
    });

    if (!response.ok)
    {
        displayError('Failed to upload file.');
        throw new Error('Failed to upload file to MongoDB collection.');
    }

    const data = await response.json();
    return data.data.fileExists;
}
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

hideLoader();

documentNameInput.addEventListener('keyup',() =>
{
    validateForm();
});

documentTypeInput.addEventListener('change',() =>
{
    validateForm();
});

documentDescriptionInput.addEventListener('keyup',() =>
{
    validateForm();
});

function validateForm()
{
    const documentName = documentNameInput.value.trim();
    const documentType = documentTypeInput.value;
    const documentDescription = documentDescriptionInput.value.trim();

    nextButton.disabled = !(documentName && documentType && documentDescription);
    checkNextActivation();
}

validateForm();

async function storeDocumentDescription()
{
    const documentName = document.getElementById('documentName').value;
    const documentType = document.getElementById('documentType').value;
    const documentDescription = document.getElementById('documentDescription').value;

    const documentData = {
        documentName: documentName,
        documentType: documentType,
        documentDescription: documentDescription
    };

    const jsonData = JSON.stringify(documentData);
    sessionStorage.setItem('documentContext',jsonData);
}

async function customKeyword()
{
    const fileName = sessionStorage.getItem('selectedFile');
    const namespace = sessionStorage.getItem('sessionNamespace');
    const requestBody = {
        namespace: namespace,
        file_name: fileName
    };
    showLoader();
    await fetch("https://strive-core.azurewebsites.net/keywordsModel",{
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
                displayError('Failed to process file.')
                throw new Error(`HTTP error! Status: ${response.status}`);
            }

            return response.text();
        })
        .then(data =>
        {
            sessionStorage.setItem('keywords',data);
        })
        .catch(error =>
        {
            hideLoader();
            displayError('Failed to process file.');
            console.error('Fetch error:',error);
        });
    hideLoader();
}

async function uploadDocumentToApplication()
{
    const fileInput = document.createElement("input");
    fileInput.type = "file";

    fileInput.click();

    fileInput.addEventListener("change",async (event) =>
    {
        const selectedFile = event.target.files[0];
        const allowedExtensions = ['pdf'];
        const fileExtension = selectedFile.name.split('.').pop().toLowerCase();
        if (!allowedExtensions.includes(fileExtension))
        {
            alert("Only PDF, DOCX, DOC files are allowed.");
            return;
        }
        sessionStorage.setItem('selectedFile',selectedFile.name);
        var fileExists = await validateFileExistence();
        if (fileExists)
        {
            displayError('File already exists.');
            return;
        }
        const tableBody = document.getElementById('uploadedFilesTableBody');
        const tableRow = document.createElement('tr');

        const fileNameCell = document.createElement('td');
        fileNameCell.textContent = selectedFile.name;

        const actionsCell = document.createElement('td');
        const deleteButton = document.createElement('button');
        deleteButton.textContent = 'Delete';
        deleteButton.classList.add('btn','btn-danger');
        deleteButton.addEventListener('click',() =>
        {
            deleteFileFromMongo(selectedFile.name,sessionStorage.getItem('sessionNamespace'),tableRow);
            sessionStorage.removeItem('selectedFile');
            //deleteFile(selectedFile.name,tableRow);
        });
        actionsCell.appendChild(deleteButton);

        const selectCell = document.createElement('td');
        const selectRadio = document.createElement('input');
        selectRadio.type = 'radio';
        selectRadio.name = 'selectedFile';
        selectRadio.value = selectedFile.name;
        selectRadio.classList.add('large-radio-button');
        selectRadio.addEventListener('change',async () =>
        {
            sessionStorage.setItem('selectedFile',selectedFile.name);
            await customKeyword();
        });
        selectCell.appendChild(selectRadio);

        const statusCell = document.createElement('td');
        tableRow.appendChild(statusCell);
        statusCell.textContent = 'Uploading...';
        tableRow.appendChild(fileNameCell);
        tableRow.appendChild(actionsCell);
        tableRow.appendChild(selectCell);
        tableBody.appendChild(tableRow);

        try
        {
            showLoader();
            await sendFileToMongoDB(selectedFile);
            const uploadedFiles = JSON.parse(sessionStorage.getItem('uploadedFiles')) || [];
            uploadedFiles.push(selectedFile.name);
            sessionStorage.setItem('uploadedFiles',JSON.stringify(uploadedFiles));
            sessionStorage.setItem('selectedFile',selectedFile.name);
            await customKeyword();
            await populateUploadedFilesList();
            validateFiles();
            hideLoader();
        } catch (error)
        {
            hideLoader();
            displayError('Failed to upload or process file.');
            statusCell.textContent = 'Error';
            console.error('Error uploading file:',error);
        }
    });
}

async function populateUploadedFilesList()
{
    const namespace = sessionStorage.getItem('sessionNamespace');
    const response = await fetch(`https://strive-api.azurewebsites.net/api/MongoDB/GetAllDocuments?collectionName=${namespace}`,{
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });
    var responseData = await response.json();
    const uploadedFiles = [];
    responseData.data.forEach(document =>
    {
        if (document.fileExists)
        {
            uploadedFiles.push(document.fileName);
        }
    });

    //const uploadedFiles = JSON.parse(sessionStorage.getItem('uploadedFiles')) || [];
    const uploadedFilesTableBody = document.getElementById('uploadedFilesTableBody');
    uploadedFilesTableBody.innerHTML = ""; // Clear existing entries

    for (const fileName of uploadedFiles)
    {
        const tableRow = document.createElement('tr');
        const statusCell = document.createElement('td');
        const fileNameCell = document.createElement('td');
        fileNameCell.textContent = fileName;



        const actionsCell = document.createElement('td');
        const deleteButton = document.createElement('button');
        deleteButton.textContent = 'Delete';
        deleteButton.classList.add('btn','btn-danger');
        deleteButton.addEventListener('click',() =>
        {
            deleteFileFromMongo(fileName,sessionStorage.getItem('sessionNamespace'),tableRow);
            //deleteFile(fileName,tableRow);
        });
        actionsCell.appendChild(deleteButton);

        const selectCell = document.createElement('td');
        const selectRadio = document.createElement('input');
        selectRadio.type = 'radio';
        selectRadio.name = 'selectedFile';
        selectRadio.value = fileName;
        selectRadio.classList.add('large-radio-button');
        if (sessionStorage.getItem('selectedFile') == fileName)
        {
            selectRadio.checked = true;
        }
        selectRadio.addEventListener('change',async () =>
        {
            if (selectRadio.checked)
            {
                sessionStorage.setItem('selectedFile',fileName);
                await customKeyword();
                checkNextActivation();
            }
        });
        selectCell.appendChild(selectRadio);
        statusCell.textContent = 'Uploaded';
        tableRow.appendChild(statusCell);
        tableRow.appendChild(fileNameCell);
        tableRow.appendChild(actionsCell);
        tableRow.appendChild(selectCell);
        uploadedFilesTableBody.insertBefore(tableRow,uploadedFilesTableBody.firstChild);
    }
    validateFiles();
}

async function sendFileToMongoDB(selectedFile)
{
    const formData = new FormData();
    formData.append('targetFile',selectedFile);

    const namespace = sessionStorage.getItem("sessionNamespace");
    const url = `https://strive-api.azurewebsites.net/api/MongoDB/UploadDocument?collectionName=${namespace}`;

    const response = await fetch(url,{
        method: 'POST',
        headers: {
            'Access-Control-Allow-Origin': '*',
            'Access-Control-Allow-Methods': '*',
            'Access-Control-Allow-Headers': '*'
        },
        body: formData
    });

    if (!response.ok)
    {
        displayError('Failed to upload file.');
        throw new Error('Failed to upload file to MongoDB collection.');
    }

    const data = await response.json();
}

async function routeToDocumentAnalysis()
{
    var documentAnalysisUrl = window.location.protocol + "//" + window.location.host + '/Home/DocumentDashboard';
    window.location.href = documentAnalysisUrl;
}

async function deleteConversationMemory() {
    try {
        const response = await fetch('https://strive-core.azurewebsites.net/deleteConversationMemory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                namespace: sessionStorage.getItem('sessionNamespace')
            })
        });

        // Check if the response is OK
        if (!response.ok) {
            if (response.status === 500) {
                // Handle 500 error by logging or performing an alternative action
                console.error('Error 500: Internal Server Error. Continuing execution.');
                return; // Or you can return some default value or null
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Parse and return JSON if no error occurred
        return response.json();
    } catch (error) {
        console.error('An error occurred:', error);
        // Continue execution by returning null or an alternative value
        return null;
    }

}

async function uploadFileFlow()
{
    var fileUploadButtons = document.getElementsByClassName('file-upload-button');
    for (let btn of fileUploadButtons)
    {
        btn.disabled = true;
    }
    storeDocumentDescription();
    await deleteConversationMemory();
    await routeToDocumentAnalysis();
    localStorage.setItem('showDocumentDashboardWidget','true');
}

function showLoader()
{
    document.getElementById("page-container").style.display = 'block';
}

function hideLoader()
{
    document.getElementById("page-container").style.display = 'none';
}

async function deleteFile(fileName,tableRow)
{
    let uploadedFiles = JSON.parse(sessionStorage.getItem('uploadedFiles')) || [];
    uploadedFiles = uploadedFiles.filter(file => file !== fileName);
    sessionStorage.setItem('uploadedFiles',JSON.stringify(uploadedFiles));
    tableRow.remove();
    validateFiles();
}

async function deleteFileFromMongo(fileName,collectionName,tableRow)
{
    try
    {
        const sessionName = sessionStorage.getItem('sessionNamespace');
        var url = `https://strive-api.azurewebsites.net/api/MongoDB/DeleteAllVersions?fileName=${fileName}&collectionName=${sessionName}`;
        const response = await fetch(url,{
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Check if the response is OK
        if (!response.ok)
        {
            if (response.status === 500)
            {
                // Handle 500 error by logging or performing an alternative action
                console.error('Error 500: Internal Server Error. Continuing execution.');
                return; // Or you can return some default value or null
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        // Parse and return JSON if no error occurred
        tableRow.remove();
        return response.json();
    } catch (error)
    {
        console.error('An error occurred:',error);
        // Continue execution by returning null or an alternative value
        return null;
    }

}

window.addEventListener('load',function ()
{
    sessionStorage.removeItem('selectedFile');
    populateUploadedFilesList();
    checkNextActivation(); 
});