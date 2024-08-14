using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabController : ControllerBase
    {
        LabContext db;
        private static string ApiKey = "";
        private static string Bucket = "";
        private static string AuthEmail = "";
        private static string AuthPassword = "";
        string ConnectionString = @"";
        public LabController(LabContext context) { db = context; }

        [HttpGet("{program_id}")]
        public async Task<ActionResult<string>> GetLabs(int program_id)
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                List<Lab> labs = new List<Lab>();
                SqlCommand command = new SqlCommand(@"select lab.id, lab.name, lab.beginDate, lab.deadline, lab.link from lab join lab_program on lab.id = lab_program.lab where program = @program", connection);
                SqlParameter programParam = new SqlParameter("@program", program_id);
                command.Parameters.Add(programParam);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Lab lab = new Lab();
                    lab.id = reader.GetInt32(0);
                    lab.name = reader.GetString(1);
                    lab.beginDate = reader.GetDateTime(2);
                    lab.deadline = reader.GetDateTime(3);
                    lab.link = reader.GetString(4);
                    labs.Add(lab);
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<List<Lab>>(labs, options);
                return jsonString;
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("{program_id}")]
        public async Task<ActionResult<string>> Upload([FromForm] string json, IFormFile file, int program_id)
        {
            try
            {
                //JsonSerializerOptions options = new()
                //{
                //    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                //    WriteIndented = true
                //};
                Lab lab = JsonSerializer.Deserialize<Lab>(json)!;
                try
                {
                    var memoryStream = new MemoryStream();

                    using (var stream = file.OpenReadStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        await reader.BaseStream.CopyToAsync(memoryStream);
                    }
                    memoryStream.Position = 0;
                    //Response.Headers.Add("Content-Type", "application/octet-stream");
                    var url = await Run(memoryStream, file.FileName);
                    lab.link = url;
                    SqlConnection connection = new SqlConnection(ConnectionString);
                    connection.Open();
                    SqlCommand command = new SqlCommand(@"begin transaction
                    insert lab values (@name, @deadline, @link, @beginDate)
                    IF (@@error <> 0)
                        ROLLBACK
                
                    insert lab_program values ((select top 1 id from lab order by id desc), @program)
                     IF (@@error <> 0)
                        ROLLBACK
                    commit", connection);
                    SqlParameter nameParam = new SqlParameter("@name", lab.name);
                    SqlParameter deadlineParam = new SqlParameter("@deadline", lab.deadline);
                    SqlParameter linkParam = new SqlParameter("@link", lab.link);
                    SqlParameter beginParam = new SqlParameter("@beginDate", lab.beginDate);
                    SqlParameter programParam = new SqlParameter("@program", program_id);
                    command.Parameters.Add(nameParam);
                    command.Parameters.Add(deadlineParam);
                    command.Parameters.Add(linkParam);
                    command.Parameters.Add(beginParam);
                    command.Parameters.Add(programParam);
                    command.ExecuteReader();
                    connection.Close();
                    connection = new SqlConnection(ConnectionString);
                    connection.Open();
                    command = new SqlCommand("select top 1 id from lab order by id desc",connection);
                    var reader1 = command.ExecuteReader();
                    while (reader1.Read())
                    {
                        lab.id = reader1.GetInt32(0);
                    }
                    connection.Close();
                    JsonSerializerOptions options = new()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                        WriteIndented = true
                    };
                    var jsonString = JsonSerializer.Serialize<Lab>(lab, options);
                    return jsonString;
                    //this.Response.Headers.Add("Content-Disposition", $"attachment;filename={file.FileName}");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }
        private static async Task<string> Run(Stream stream, string filename)
        {

            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("labs")
                .Child(filename)
                .PutAsync(stream, cancellation.Token, "application/force-download");
            try
            {
                string downloadUrl = await task;
                return (downloadUrl);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet("{teacher_id}/{program_id}")]
        public async Task<ActionResult<string>> GetProgramLabs(int teacher_id, int program_id)
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                Programm programm = new Programm();
                SqlCommand command = new SqlCommand(@"select course, sem, s.name, l.id, l.name,l.beginDate, l.deadline, l.link
                from program p
                left join subject_program sp
                on p.id = sp.program
                left join subject s
                on s.id = sp.subject
                left join lab_program lp
                on p.id = lp.program
                left join lab l
                on l.id = lp.lab
                where creator = @teacher and p.id = @program", connection);
                SqlParameter programParam = new SqlParameter("@program", program_id);
                SqlParameter teacherParam = new SqlParameter("@teacher", teacher_id);
                command.Parameters.Add(programParam);
                command.Parameters.Add(teacherParam);
                var reader = command.ExecuteReader();
                programm.labs = new List<Lab>();
                while (reader.Read())
                {
                    programm.course = reader.GetInt32(0);
                    programm.sem = reader.GetInt32(1);
                    programm.subject = reader.GetString(2);
                    Lab lab = new Lab();
                    lab.id = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                    lab.name = reader.IsDBNull(4) ? null : reader.GetString(4);
                    lab.beginDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5);
                    lab.deadline = reader.IsDBNull(6) ? null : reader.GetDateTime(6);
                    lab.link = reader.IsDBNull(7) ? null : reader.GetString(7);
                    if (lab.id != null && lab.link != null)
                    {
                        programm.labs.Add(lab);
                    }
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<Programm>(programm, options);
                return jsonString;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{lab_id}")]
        public async Task<ActionResult<string>> UpdateLab([FromBody] Lab lab)
        {
            try
            {
                //Lab lab = JsonSerializer.Deserialize<Lab>(json);
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = new SqlCommand(@"update lab set name = @name, beginDate = @beginDate, deadline = @deadline where id = @id", connection);
                SqlParameter idParam = new SqlParameter("@id", lab.id);
                SqlParameter nameParam = new SqlParameter("@name", lab.name);
                SqlParameter beginParam = new SqlParameter("@beginDate", lab.beginDate);
                SqlParameter deadlineParam = new SqlParameter("@deadline", lab.deadline);
                command.Parameters.Add(idParam);
                command.Parameters.Add(nameParam);
                command.Parameters.Add(beginParam);
                command.Parameters.Add(deadlineParam);
                command.ExecuteReader();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteLab(int id)
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = new SqlCommand("delete from lab where id = @id", connection);
                SqlParameter idParam = new SqlParameter("@id", id);
                command.Parameters.Add(idParam);
                command.ExecuteReader();
                connection.Close();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
