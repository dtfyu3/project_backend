using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgrammController : ControllerBase
    {
        ProgrammContext db;
        string ConnectionString = @"";
        public ProgrammController(ProgrammContext context) { db = context; }
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            try
            {
                int teacher = id;
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                List<Subject> subjects = new List<Subject>();
                SqlCommand command = new SqlCommand(@"select su.name,p.id, faculty, course, sem, [group], (select count(*) from lab_program where program = p.id	) as labsCount
from program p
join subject_program s
on p.id = s.program
join subject su
on s.subject = su.id
where creator = @teacher", connection);
                SqlParameter teacherParam = new SqlParameter("@teacher", teacher);
                command.Parameters.Add(teacherParam);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Subject subject = new Subject();
                    subject.programms = new List<Programm>();
                    Programm program = new Programm();
                    subject.subject = reader.GetString(0);
                    program.Id = reader.GetInt32(1);
                    program.faculty = reader.GetString(2);
                    program.course = reader.GetInt32(3);
                    program.sem = reader.GetInt32(4);
                    program.group = reader.GetString(5);
                    program.labsCount = reader.GetInt32(6);
                    int i = 0;
                    bool flag = false;
                    int index = 0;
                    foreach (var item in subjects)
                    {
                        if (item.subject == subject.subject)
                        {
                            flag = true;
                            index = i;
                            break;
                        }
                        i++;
                    }
                    if (flag == true)
                        subjects[index].programms.Add(program);
                    else
                    {
                        subject.programms.Add(program);
                        subjects.Add(subject);
                    }
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<List<Subject>>(subjects, options);
                return jsonString;
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("subjects")]
        public async Task<ActionResult<string>> GetSubjects()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            List<string> subjects = new List<string>();
            SqlCommand command = new SqlCommand("select name from subject", connection);
            try
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string subject = reader.GetString(0);
                    subjects.Add(subject);
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<List<string>>(subjects, options);
                return jsonString;
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpPost]
        public IActionResult Post([FromBody] Programm data)
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = new SqlCommand(@"insert program values (@teacher_id,NULL,@faculty,@course,@sem,@group)",connection);
                SqlParameter teacherParam = new SqlParameter("@teacher_id",data.teacher_id);
                SqlParameter facultyParam = new SqlParameter("@faculty", data.faculty);
                SqlParameter courseParam = new SqlParameter("@course", data.course);
                SqlParameter semParam = new SqlParameter("@sem", data.sem);
                SqlParameter groupParam = new SqlParameter("@group", data.group);
                command.Parameters.Add(teacherParam);
                command.Parameters.Add(facultyParam);
                command.Parameters.Add(courseParam);
                command.Parameters.Add(semParam);
                command.Parameters.Add(groupParam);
                command.ExecuteReader();
                connection.Close();
                connection.Open();
                command = new SqlCommand(@"insert subject_program values ((select id from subject where name = @subject), (select top 1 id from program order by id desc))", connection);
                SqlParameter subjectParam = new SqlParameter("@subject", data.subject);
                command.Parameters.Add(subjectParam);
                command.ExecuteReader();
                connection.Close();
                connection.Open();
                command = new SqlCommand(@"select top 1 id from program order by id desc", connection);
                var reader = command.ExecuteReader();
                int id = -1;
                while (reader.Read()) { id = reader.GetInt32(0); }
                connection.Close();
                return Ok($"{id}");
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<string>> DeleteProgram(int id)
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = new SqlCommand("delete from program where id = @id", connection);
                SqlParameter programParam = new SqlParameter("@id", id);
                command.Parameters.Add(programParam);
                command.ExecuteReader();
                connection.Close();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet("student/subjects")]
        public async Task<ActionResult<string>> GetStudentSubjects()
        {
            try
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                SqlCommand command = new SqlCommand(@"select distinct s.name, u.surname
                from subject s
                left join subject_program sp
                on sp.subject = s.id
                left join program p
                on sp.program = p.id
                left join users u
                on u.id = p.creator ", connection);
                var reader = command.ExecuteReader();
                List<Subject> subjects = new List<Subject>();
                while (reader.Read())
                {
                    Subject subject = new Subject();
                    subject.subject = reader.GetString(0);
                    subject.teacher = reader.IsDBNull(1) ? null : reader.GetString(1);
                    subjects.Add(subject);
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<List<Subject>>(subjects, options);
                return jsonString;
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
