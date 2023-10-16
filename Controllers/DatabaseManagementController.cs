using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using Trekkers_AA.Models;
using Microsoft.AspNetCore.Identity;

namespace Trekkers_AA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseManagementController : Controller
    {
        private readonly IMongoDatabase _database;
        public DatabaseManagementController()
        {
            var client = new MongoClient("mongodb+srv://trekkers:trekkers@serviceaccess.lovdwri.mongodb.net/?retryWrites=true&w=majority");
            _database = client.GetDatabase("UserAccess");
        }
        
        [HttpGet]
        public ActionResult<IEnumerable<UserModel>> GetUser(string email)
        {
            var collection = _database.GetCollection<UserModel>("UserCredentials");
            var model = collection.Find(user => user.email == email).FirstOrDefault();

            if (model == null) {
                return NotFound();
            }

            return Ok(model);
        }

        [HttpPut]
        public ActionResult<IEnumerable<UserModel>> UpdateUserPassword(string email, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Email and new password are required.");
            }

            var collection = _database.GetCollection<UserModel>("UserCredentials");

            var filter = Builders<UserModel>.Filter.Eq(u => u.email, email);

            var update = Builders<UserModel>.Update.Set(u => u.password, newPassword);

            var result = collection.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("User not found or password not updated.");
            }

        }

    }
}
