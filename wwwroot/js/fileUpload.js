
function deleteSelectedFile() {
    const selectedFileName = document.getElementById('selected-file-name');
    const deleteFileButton = document.getElementById("delete-button"); 
    deleteFileButton.addEventListener("click", deleteSelectedFile);
    const requestBody = {
        FileName: localStorage.getItem("selectedFiles")
    };
    fetch(window.location.protocol + "//" + window.location.host + "/api/fileupload/delete",{
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
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
            return response.json();
        })
        .then(data =>
        {
            console.log(data);
        })
        .catch(error =>
        {
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
    localStorage.removeItem("selectedFiles",selectedFileName);

}

function uploadDocumentToApplication()
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

        if (!allowedExtensions.includes(fileExtension)) {
            displayError("Only PDF, DOCX, DOC files are allowed.");
            return;
        }
        
        if (selectedFile)
        {
            selectedFileName.textContent = "Selected file: " + selectedFile.name;
            localStorage.setItem("selectedFiles",selectedFile.name);
            if (localStorage.getItem("selectedFiles") !== null)
            {
                document.getElementById("next-button").removeAttribute("disabled");
                // Show delete button upon file selection
                deleteButton.classList.remove('d-none');
            }


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

                })
                .then(data => console.log(data))
                .catch(error =>
                {
                    hideLoader();
                    displayError("System is under maintenance. Please try again later.")
                    console.log('Upload error:',error);
                });
            hideLoader();
        }
        
    });
    fileInput.click(); // Trigger the file selection dialog
}



function uploadDocumentToPinecone()
{
    const requestBody = {
        Namespace: "Deven",
        FileName: localStorage.getItem("selectedFiles")
    };
    showLoader();
    fetch(window.location.protocol + "//" + window.location.host + "/api/indexer/insertDocument",{
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(requestBody)
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
            hideLoader();
            console.log(data);
        })
        .catch(error =>
        {
            hideLoader();
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
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
    document.getElementById("loader-spinner").style.display = "";
    document.getElementById("page-container").style.opacity = 0.5;
}

function hideLoader()
{
    document.getElementById("loader-spinner").style.display = "none";
    document.getElementById("page-container").style.opacity = 1;

}

window.onload = function ()
{
    hideLoader();
};
