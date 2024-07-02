$(document).ready(function ()
{
    if ('@User.Identity.IsAuthenticated' === 'True' && localStorage.getItem('userFirstName') == null)
    {
        $.ajax({
            url: 'https://strive-api.azurewebsites.net/api/user/getUser?username=' + '@User.Identity.Name',
            type: 'GET',
            success: function (data)
            {
                localStorage.setItem('userFirstName',data['data']['firstName']);
                $('#user-greeting').text(localStorage.getItem('userFirstName'));
            }
        });
    }
    else
    {
        $('#user-greeting').text(localStorage.getItem('userFirstName'));
    }
});