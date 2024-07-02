function openNav()
{
    document.body.classList.add('sidebar-open');
    document.getElementById("mySidebar").style.width = "250px";
    document.getElementById("main-content").style.marginLeft = "250px";
    document.getElementById("main-content").style.width = "calc(100% - 250px)";
}

function closeNav()
{
    document.body.classList.remove('sidebar-open');
    document.getElementById("mySidebar").style.width = "0";
    document.getElementById("main-content").style.marginLeft = "0";
    document.getElementById("main-content").style.width = "100%";
}

// Call openNav function on page load
$(document).ready(function ()
{
    openNav();
});