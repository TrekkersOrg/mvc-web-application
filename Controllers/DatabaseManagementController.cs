﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Trekkers_AA.Models;
using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace Trekkers_AA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly MongoClient _client;

        // Constructor: Initializes the controller and establishes a connection to the MongoDB database.
        public DatabaseManagementController(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new MongoClient(_configuration["ConnectionStrings:DefaultConnection"]);
        }

        // HTTP GET request handler: Retrieves user information by email.
        [HttpGet]
        public ActionResult<IEnumerable<UserModel>> GetUser(string email)
        {
            // Connect to database
            IMongoDatabase userAccessDatabase = _client.GetDatabase("UserAccess");

            // Validate the email address using the emailValidator function.
            if (!emailValidator(email))
            {
                return BadRequest("Invalid email.");
            }

            // Get the MongoDB collection and find a user by their email.
            var collection = userAccessDatabase.GetCollection<UserModel>("UserCredentials");
            var model = collection.Find(user => user.email == email).FirstOrDefault();

            // If the user is not found, return a 404 Not Found response; otherwise, return the user data.
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        // HTTP PUT request handler: Updates a user's password based on their email.
        [HttpPut]
        public ActionResult<IEnumerable<UserModel>> UpdateUserPassword(string email, string newPassword)
        {
            // Connect to database
            IMongoDatabase userAccessDatabase = _client.GetDatabase("UserAccess");

            // Validate that both email and newPassword are provided.
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Email and new password are required.");
            }

            // Get the MongoDB collection, define a filter to find the user by email, and specify the update operation.
            var collection = userAccessDatabase.GetCollection<UserModel>("UserCredentials");
            var filter = Builders<UserModel>.Filter.Eq(u => u.email, email);
            var update = Builders<UserModel>.Update.Set(u => u.password, newPassword);

            // Perform the update operation and check the result.
            var result = collection.UpdateOne(filter, update);

            // If the password is updated successfully, return a 200 OK response; otherwise, return a 404 Not Found response.
            if (result.ModifiedCount > 0)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("User not found or password not updated.");
            }
        }

        [HttpPost]
        public ActionResult<IEnumerable<SessionModel>> CreateUserSession(string email, string file)
        {
            // Connect to database
            IMongoDatabase userAccessDatabase = _client.GetDatabase("UserAccess");

            // Get the MongoDB collection
            var collection = userAccessDatabase.GetCollection<UserModel>("UserSession");
        }

        [Route("session")]
        [HttpDelete]
<<<<<<< HEAD
        public async Task<ActionResult> DeleteUserSession(string sessionId)
=======
        public ActionResult<IEnumerable<SessionModel>> DeleteUserSession(string email, string file)
        {

        }

        [HttpPost]
        public ActionResult<IEnumerable<DebugLogModel>> PostLog(ObjectId sessionId)
>>>>>>> d8eb9de4a8f82af06411b0a43cd2845598a5a63b
        {
            try
            {
                var objectId = new ObjectId(sessionId);

                // Connect to UserAccess collection
                IMongoDatabase userAccessDatabase = _client.GetDatabase("UserAccess");

                // Retrieve the MongoDB collection
                var collection = userAccessDatabase.GetCollection<SessionModel>("UserSession");

                // Create a filter to find the session with the given sessionId
                var filter = Builders<SessionModel>.Filter.Eq("_id", objectId);

                // Delete the session
                var result = await collection.DeleteOneAsync(filter);

                // Error
                if (result.DeletedCount > 0)
                {
                    return Ok(new { Success = true, Message = "Session deleted successfully" });
                }

                return NotFound(new { Success = false, Message = "Session not found" });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        // Custom function for email validation: Uses a regular expression to validate email addresses.
        public bool emailValidator(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }

    }
}
