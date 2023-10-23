function validateUser()
{
    document.getElementById("loader-id").style.display = "flex";
    
    document.getElementById("error").style.display = "none";
    document.getElementById("email").style.backgroundColor = "white";
    document.getElementById("password").style.backgroundColor = "white";
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!document.getElementById("email").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Email is required.";
        document.getElementById("error").style.display = "block";
        document.getElementById("email").style.backgroundColor = "#FFB3B3";
        return;
    }
    if (!document.getElementById("password").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Password is required.";
        document.getElementById("error").style.display = "block";
        document.getElementById("password").style.backgroundColor = "#FFB3B3";
        return;
    }
    if (!emailPattern.test(document.getElementById("email").value))
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Invalid email";
        document.getElementById("error").style.display = "block";
        document.getElementById("email").style.backgroundColor = "#FFB3B3";
        return;
    }
    fetch('https://localhost:7114/api/databasemanagement?email=' + document.getElementById("email").value,{
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response =>
        {
            document.getElementById("loader-id").style.display = "none";
            return response.json();
        })
        .then(data =>
        {
            document.getElementById("loader-id").style.display = "none";
            userData = data;
            if (document.getElementById("password").value == userData["password"])
            {
                window.location.assign('Test');
            } else {
                document.getElementById("error").textContent = "Invalid login";
                document.getElementById("error").style.display = "block";
                document.getElementById("email").style.backgroundColor = "#FFB3B3";
                document.getElementById("password").style.backgroundColor = "#FFB3B3";
                return;
            }
        })
        .catch(error =>
        {
            document.getElementById("loader-id").style.display = "none";
            console.error('There was a problem with the API request:',error);
        });
}