function changePassword()
{
    document.getElementById("loader-id").style.display = "flex";
    
    document.getElementById("error").style.display = "none";
    document.getElementById("email").style.backgroundColor = "white";
    document.getElementById("new-password").style.backgroundColor = "white";
    document.getElementById("confirm-password").style.backgroundColor = "white";
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!document.getElementById("email").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Email is required.";
        document.getElementById("error").style.display = "block";
        document.getElementById("email").style.backgroundColor = "#FFB3B3";
        return;
    }

    if (!document.getElementById("new-password").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Password is required.";
        document.getElementById("error").style.display = "block";
        document.getElementById("new-password").style.backgroundColor = "#FFB3B3";
        return;
    }

    if (!document.getElementById("confirm-password").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Password is required.";
        document.getElementById("error").style.display = "block";
        document.getElementById("confirm-password").style.backgroundColor = "#FFB3B3";
        return;
    }

    if (document.getElementById("new-password").value != document.getElementById("confirm-password").value)
    {
        document.getElementById("loader-id").style.display = "none";
        document.getElementById("error").textContent = "Passwords do not match.";
        document.getElementById("error").style.display = "block";
        document.getElementById("new-password").style.backgroundColor = "#FFB3B3";
        document.getElementById("confirm-password").style.backgroundColor = "#FFB3B3";
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

    fetch('/api/databasemanagement?email=' + document.getElementById("email").value + '&newPassword=' + document.getElementById("confirm-password").value,{
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
    })
        .then(response =>
        {
            document.getElementById("loader-id").style.display = "none";
            if (response.status == 404)
            {
                document.getElementById("error").textContent = "Enter a new password.";
                document.getElementById("error").style.display = "block";
                document.getElementById("new-password").style.backgroundColor = "#FFB3B3";
                document.getElementById("confirm-password").style.backgroundColor = "#FFB3B3";
                return;
            }
            return response.json();
        })
        .then(data =>
        {
            document.getElementById("loader-id").style.display = "none";
            userData = data;
            console.log(userData);
            if (userData["matchedCount"] == 1 && userData["modifiedCount"] == 1)
            {
                window.location.assign('Login');
            } else {
                document.getElementById("error").textContent = "Invalid login";
                document.getElementById("error").style.display = "block";
                document.getElementById("email").style.backgroundColor = "#FFB3B3";
                document.getElementById("new-password").style.backgroundColor = "#FFB3B3";
                return;
            }
        })
        .catch(error =>
        {
            document.getElementById("loader-id").style.display = "none";
            console.error('There was a problem with the API request:',error);
        });
}