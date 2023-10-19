document.addEventListener("DOMContentLoaded",function ()
{
    function getUser()
    {
        console.log("Get user");
        const email = document.getElementById("get-email").value;

        fetch('https://localhost:7114/api/databasemanagement?email=' + email,{
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
            .then(user =>
            {
                findMatch(user);
            })
            .catch(error =>
            {
                console.error('There was a problem with the API request:',error);
            });
    }

    function findMatch(user)
    {
        console.log("Find match");
        const emailInput = document.getElementById("get-email");
        const passwordInput = document.getElementById("get-password");

        if (user.email === emailInput.value)
        {
            if (user.password === passwordInput.value)
            {
                alert("Successful login!");
            } else
            {
                alert("Invalid password");
            }
        } else
        {
            console.log("Invalid email");
        }
    }
});
