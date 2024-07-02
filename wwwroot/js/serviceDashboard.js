document.addEventListener('DOMContentLoaded',function ()
{
    const allFilesWidget = document.getElementById('all-files-widget');

    function fetchUploadedFiles()
    {
        const uploadedFiles = JSON.parse(sessionStorage.getItem('uploadedFiles')) || [];
        let widgetContent = `
                <div class="widget shadow p-3 mb-5 bg-light rounded">
                    <h5 class="widget-title">All Files</h5>
                    <ul class="list-group">`;

        uploadedFiles.forEach(file =>
        {
            widgetContent += `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${file}
                        <button class="btn btn-primary btn-sm" onclick="analyzeFile('${file}')">Analyze</button>
                    </li>`;
        });

        widgetContent += `
                    </ul>
                </div>`;

        allFilesWidget.innerHTML = widgetContent;
    }

    window.analyzeFile = function (fileName)
    {
        sessionStorage.setItem('selectedFile',fileName);
        window.location.href = '@Url.Action("DocumentDashboard", "Home")';
    }

    fetchUploadedFiles();
});