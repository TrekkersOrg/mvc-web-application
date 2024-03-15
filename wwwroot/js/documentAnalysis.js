const { Session } = require("inspector");

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
    document.getElementById("summary-box").innerText = sessionStorage.getItem("documentSummary");
}

window.onload = function ()
{
    console.log("HI");
    document.addEventListener("DOMContentLoaded",function ()
    {
        loadSummary();
    });
    hideLoader();
};
