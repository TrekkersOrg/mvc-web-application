// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function() {
    // Add active class to nav-link based on current URL
    var pathname = window.location.pathname;  // Get current page path
    $('.navbar-nav .nav-link').each(function() {
        var href = $(this).attr('href');
        if (href === pathname) {
            $(this).addClass('active');
        }
    });

    // Handle clicks on nav-links
    $('.navbar-nav .nav-link').click(function() {
        $('.navbar-nav .nav-link').removeClass('active');
        $(this).addClass('active');
    });
});
