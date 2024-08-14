using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        UsersContext db;
        string ConnectionString = @"";
        public UsersController(UsersContext context)
        {
            db = context;
        }
        //public UsersController(IUserRepository userRepository)
        //{
        //    this.userRepository = userRepository;
        //}



        //[HttpGet]
        //public async Task<ActionResult> GetUsers()
        //{
        //    try
        //    {
        //        return Ok(await userRepository.GetUsers());
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            "Error retrieving data from the database");
        //    }
        //}
        // GET api/users/5
        [HttpGet("{login}")]
        public async Task<ActionResult<string>> Get(string login)

        {
            string log = "";
            string pas = "";
            string[] arr = login.Split('@');
            if (arr.Length > 1)
            {
                log = arr[0];
                pas = arr[1];
                if (login != "" & pas != "")
                {
                    SqlConnection connection = new SqlConnection(ConnectionString);
                    connection.Open();
                    string sql = @"select u.id,concat(u.surname,' ', u.name) as fi, u.[group] as gruppa, u.type
                from auth a
                join users u
                on a.[user] = u.id
				where a.login = @login and a.password = @pas";
                    SqlParameter logParam = new SqlParameter("@login", log);
                    SqlParameter pasParam = new SqlParameter("@pas", pas);
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.Add(logParam);
                    command.Parameters.Add(pasParam);
                    var reader = command.ExecuteReader();
                    User user = new User();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string surname_name = reader.GetString(1);
                        try { int group = reader.GetInt32(2);user.Group = group; }
                        catch (Exception ex) { user.Group = null; }
                        bool type = Convert.ToBoolean(reader.GetValue(3));
                        user.Id = id;
                        user.Surname_Name = surname_name;
                        user.Type = type;
                    }
                    connection.Close();
                    JsonSerializerOptions options = new()
                    {
                        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                        WriteIndented = true
                    };
                    if (user.Id != null)
                    {
                        var jsonString = System.Text.Json.JsonSerializer.Serialize<User>(user, options);
                        return  jsonString;
                    }
                    else return BadRequest();
                }

                return BadRequest();

            }
            else return BadRequest();
        }

        // POST api/users
        [HttpPost]
        public async Task<ActionResult<User>> Post(User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            if (db.users.Contains(user) == false)
            {
                db.users.Add(user);
                await db.SaveChangesAsync();
                return Ok(user);
            }
            else return Ok("This user is already exists!");
        }

        // PUT api/users/
        [HttpPut]
        public async Task<ActionResult<User>> Put(User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            if (!db.users.Any(x => x.Id == user.Id))
            {
                return NotFound();
            }

            db.Update(user);
            await db.SaveChangesAsync();
            return Ok(user);
        }

        // DELETE api/users/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult<User>> Delete(int id)
        //{
        //    User user = db.users.FirstOrDefault(x => x.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    db.users.Remove(user);
        //    await db.SaveChangesAsync();
        //    return Ok(user);
        //}
    }
}
