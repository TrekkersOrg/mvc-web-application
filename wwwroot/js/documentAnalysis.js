
function generateSummary()
{
    const summaryParagraph = document.querySelector(".insert-summary p");
    const requestBody = {
        Vectorstore: "TestSuite",
        Query: "Generate a brief yet informative summary about this especially the critical details (e.g., dates, people, references). Make sure your summary does not exceed 250 words."
    };
    showLoader();
    fetch(window.location.protocol + "//" + window.location.host + "/api/chatbot/sendQuery",{
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
            localStorage.setItem("documentSummary",data["data"]["response"]);
        })
        .catch(error =>
        {
            hideLoader();
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });
}
