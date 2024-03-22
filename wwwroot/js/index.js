async function sendContactUsEmail()
{
    var name = document.getElementById('name').value;
    var email = document.getElementById('email').value;
    var useCase = document.getElementById('usecase').value;
    var internalEmail = 'business@trekkers789.onmicrosoft.com';
    var subjectLine = 'Inquiry from ' + name;
    var bodyLine = 'The following is an inquiry from ' + name + ' (' + email + '): ' + useCase;

    const requestBody = {
        fromEmail: internalEmail,
        fromName: 'Strive',
        toEmail: internalEmail,
        toName: 'Strive',
        subject: subjectLine,
        body: bodyLine
    };
    await fetch("/api/email/sendEmail",{
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
                sessionStorage.setItem("sendEmailStatus","fail");
                displayError("System is under maintenance. Please try again later.")
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            sessionStorage.setItem("sendEmailStatus","success");
            return response.json();
        })
        .then(data =>
        {
            sessionStorage.setItem("sendEmailStatus","success");
            console.log(data);
        })
        .catch(error =>
        {
            sessionStorage.setItem("sendEmailStatus","fail");
            hideLoader();
            displayError("System is under maintenance. Please try again later.")
            console.error('Fetch error:',error);
        });


}