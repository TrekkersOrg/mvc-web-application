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
