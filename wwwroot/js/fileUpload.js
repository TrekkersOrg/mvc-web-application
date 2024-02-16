function openFileDialog() {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = '.pdf'; 

    fileInput.addEventListener('change', (event) => {
        const selectedFile = event.target.files[0];

        if (!selectedFile || selectedFile.type !== 'application/pdf') {
            alert('Please select a valid PDF file.');
        return;
        }

        // Update the selected file name display
        const fileNameDisplay = document.getElementById('selected-file-name');
        if (selectedFile) {
            fileNameDisplay.textContent = selectedFile.name;
        }

    // Call your server-side upload function (omitted)

    });

    fileInput.onclick(); 
}

function uploadDocument()
{
    const requestBody = {
        Namespace: "Deven",
        FileName: "contract.pdf"
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
