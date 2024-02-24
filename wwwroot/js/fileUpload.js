localStorage.removeItem("selectedFiles");

function uploadDocumentToApplication()
{
    const uploadButton = document.getElementById('upload-button');
    const selectedFileName = document.getElementById('selected-file-name');
    const fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.addEventListener("change",(event) =>
    {
        const selectedFile = event.target.files[0];
        if (selectedFile)
        {
            selectedFileName.textContent = "Selected file: " + selectedFile.name;
            localStorage.setItem("selectedFiles",selectedFile.name);
            if (localStorage.getItem("selectedFiles") !== null)
            {
                document.getElementById("next-button").removeAttribute("disabled");
            }

            // Send the selected file to the API for server-side execution
            const formData = new FormData();
            formData.append('targetFile',selectedFile);
            fetch('/api/fileupload/upload',{
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => console.log(data))
                .catch(error => console.error('Upload error:',error));
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
    fetch(window.location.protocol + "//" + window.location.host + "/api/indexer/insertDocument",{
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
            console.error('Fetch error:',error);
        });
}
