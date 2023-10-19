function validateUser()
{
    fetch('https://localhost:7114/api/databasemanagement?email=' + document.getElementById("email").value,{
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response =>
        {
            if (!response.ok)
            {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data =>
        {
            userData = data;
            var emailEntry = document.getElementById("email").value;
            if (document.getElementById("password").value == userData["password"])
            {
                alert("Success!")
            }
        })
        .catch(error =>
        {
            console.error('There was a problem with the API request:',error);
        });
}