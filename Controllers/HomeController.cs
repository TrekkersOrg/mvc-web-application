﻿using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;

namespace StriveAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

// Rendering The Team Page
        public IActionResult AboutUs()
        {
            return View();
        }

// Rendering OurProducts Page
        public IActionResult OurProducts()
        {
            return View();
        }

        public IActionResult TESTING()
        {
            return View();
        }
        public IActionResult FileUpload()
        {
            return View();
        }
        public IActionResult DocumentAnalysis()
        {
            return View();
        }

        public IActionResult DocAnalysis()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}