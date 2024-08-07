﻿using Microsoft.AspNetCore.Mvc;
using StriveAI.Models;
using System.Diagnostics;

namespace StriveAI.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Demo()
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

        public IActionResult IssueReport()
        {
            return View();
        }

        public IActionResult DocumentDashboard()
        {
            return View();
        }

        // Service Dashboard config
        public IActionResult ServiceDashboard() 
        {
            return View();
        }

        public IActionResult LearnMore() 
        {
            return View();
        }

        public IActionResult ContactUs()
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