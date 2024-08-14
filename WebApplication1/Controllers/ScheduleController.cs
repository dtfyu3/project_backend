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
    public class ScheduleController : ControllerBase
    {
        ScheduleContext db;
        string ConnectionString = @"";
        public ScheduleController(ScheduleContext context) { db = context; }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            List<Schedule> schedules = new List<Schedule>();
            SqlCommand command = new SqlCommand(@"select s.id, week_number, day, g.name as groups, u.surname as teacher, sub.name as subject, class, b.name as building, number, lecture 
            from schedule s
            join groups g 
            on s.[group] = g.id
            join users u 
            on s.teacher = u.id
            join subject sub
            on s.subject = sub.id
            join building b
            on s.building = b.id", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Schedule schedule = new Schedule();
                schedule.lessons = new List<Lesson>();
                schedule.Day = reader.GetDateTime(2);
                schedule.Week_number = Convert.ToInt32(reader.GetValue(1));
                Lesson lesson = new Lesson();
                lesson.Num = Convert.ToInt32(reader.GetValue(8));
                lesson.Subject = reader.GetValue(5).ToString();
                lesson.Lecture = Convert.ToBoolean(reader.GetValue(9));
                lesson.Teacher = reader.GetValue(4).ToString();
                lesson.Class = reader.GetValue(6).ToString();
                lesson.Building = reader.GetValue(7).ToString();
                int i = 0;
                bool flag = false;
                int index = 0;
                foreach (var item in schedules)
                {
                    if (item.Day == schedule.Day)
                    {
                        flag = true;
                        index = i;
                        break;
                    }
                    i++;
                }
                if (flag == true)
                {
                    schedules[index].lessons.Add(lesson);
                }
                else
                {
                    schedule.lessons.Add(lesson);
                    schedules.Add(schedule);
                }
            }
            connection.Close();
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            var jsonString = JsonSerializer.Serialize<List<Schedule>>(schedules, options);
            return jsonString;
        }
        [HttpGet("{input}")]
        public async Task<ActionResult<string>> Get(string input)
        {
            int group = -1;
            int week_number = -1;
            string[] arr = input.Split('@');
            if (arr.Length > 1)
            {
                group = Convert.ToInt32(arr[0]);
                week_number = Convert.ToInt32(arr[1]);
                if (group != -1 && week_number != -1)
                {
                    string course = "";
                    int sem = 0;
                    SqlConnection connection = new SqlConnection(ConnectionString);
                    connection.Open();
                    SqlCommand command = new SqlCommand(@$"select substring(name,3,2) from groups where id = {group}", connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read()) { course = reader.GetString(0); };
                    course = "20" + course;
                    DateTime curDate = DateTime.Now;
                    int year = curDate.Year;
                    int month = curDate.Month;
                    if (month >= 9 && month <= 12) sem = 1;
                    else if (month >= 1 && month <= 8) sem = 2;
                    course = (year - Convert.ToInt32(course) + 1).ToString();
                    connection.Close();
                    List<Schedule> schedules = new List<Schedule>();
                    connection = new SqlConnection(ConnectionString);
                    connection.Open();
                    command = new SqlCommand(@$"select distinct schedule.id, week_number, day, teacher, schedule.subject, class, building, number, lecture, p.id, schedule.u_id
            from (select s.id, week_number, day, u.surname as teacher,u.id as u_id, sub.name as subject,sub.id as sub_id, class, b.name as building, number, lecture
            from schedule s
            join users u
            on s.teacher = u.id
            join subject sub
            on s.subject = sub.id
            join building b
            on s.building = b.id
            where s.[group] = @group and week_number = @week_number) as schedule
            left join subject_program sp
            on schedule.sub_id = sp.subject
            left join program p
            on sp.program = p.id and p.creator = schedule.u_id and p.course = @course and p.sem = @sem", connection);
                    SqlParameter groupParam = new SqlParameter("@group", group);
                    SqlParameter weekParam = new SqlParameter("@week_number", week_number);
                    SqlParameter courseParam = new SqlParameter("@course", course);
                    SqlParameter semParam = new SqlParameter("@sem", sem);
                    command.Parameters.Add(groupParam);
                    command.Parameters.Add(weekParam);
                    command.Parameters.Add(courseParam);
                    command.Parameters.Add(semParam);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        //bool newflag = false;
                        Schedule schedule = new Schedule();
                        //int id = reader.GetInt32(0);
                        //foreach(var item in ids) { if (item == id) { newflag = true; break; } }
                        //if (newflag == true) continue;
                        //ids.Add(id);
                        //schedule.Id = reader.GetInt32(0);
                        schedule.lessons = new List<Lesson>();
                        schedule.Day = reader.GetDateTime(2);
                        schedule.Week_number = Convert.ToInt32(reader.GetValue(1));
                        Lesson lesson = new Lesson();
                        lesson.Id = reader.GetInt32(0);
                        lesson.Num = Convert.ToInt32(reader.GetValue(7));
                        lesson.Subject = reader.GetValue(4).ToString();
                        lesson.Lecture = Convert.ToBoolean(reader.GetValue(8));
                        lesson.Teacher = reader.GetValue(3).ToString();
                        lesson.Teacher_id = reader.GetInt32(10);
                        lesson.Class = reader.GetValue(5).ToString();
                        lesson.Building = reader.GetValue(6).ToString();
                        lesson.program = reader.IsDBNull(9) ? null : reader.GetInt32(9);
                        int i = 0;
                        int j = 0;
                        bool flag = false;
                        bool flag1 = false;
                        int index = 0;
                        int pindex = 0;
                        foreach (var item in schedules)
                        {
                            flag1 = false;
                            var temp = item.lessons;
                            if (item.Day == schedule.Day)
                            {
                                foreach (var les in temp)
                                {
                                    if (les.Id == lesson.Id)
                                    {
                                        flag1 = true;
                                        pindex = j;
                                        break;
                                    }
                                    j++;
                                }
                                flag = true;
                                index = i;
                                break;
                            }
                            i++;
                        }

                        if (flag == true && flag1 == false)
                        {
                            schedules[index].lessons.Add(lesson);
                        }
                        else if (flag == true && flag1 == true)
                        {
                            //int j = schedules[index].lessons.FindIndex(l => l.Id == lesson.Id);
                            schedules[index].lessons[pindex] = lesson;
                        }
                        else if (flag == false)
                        {
                            schedule.lessons.Add(lesson);
                            schedules.Add(schedule);
                        }
                    }
                    connection.Close();
                    JsonSerializerOptions options = new()
                    {
                        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                        WriteIndented = true
                    };
                    var jsonString = JsonSerializer.Serialize<List<Schedule>>(schedules, options);
                    return jsonString;
                }
                return BadRequest();
            }
            else return BadRequest();
        }
        [HttpGet("teachers/{teacher_id}")]
        public async Task<ActionResult<string>> GetTeacher(int teacher_id)
        {
            try
            {
                int teacher = teacher_id;
                SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                List<Schedule> schedules = new List<Schedule>();

                SqlCommand command = new SqlCommand(@$"select distinct schedule.id, week_number, day, groups, schedule.subject, class, building, number, lecture, p.id 
            from (
            select s.id, week_number, day, g.name as groups, sub.name as subject,sub.id as sub_id, class, b.name as building, number, lecture 
            from schedule s
            left join groups g 
            on s.[group] = g.id
            left join users u 
            on s.teacher = u.id
            left join subject sub
            on s.subject = sub.id
            left join building b
            on s.building = b.id
            where u.id = @teacher_id and week_number = 2) as schedule
            left join subject_program sp on schedule.sub_id = sp.subject
            left join program p on sp.program = p.id and p.course = (year(getdate()) -    concat('20',substring(schedule.groups,3,2)) + 1)
            and p.sem = (select case when month(getdate()) >= 9 and month(getdate()) <=12 then 1 end ) order by day ", connection);
                SqlParameter teacherParam = new SqlParameter("@teacher_id", teacher);
                command.Parameters.Add(teacherParam);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Schedule schedule = new Schedule();
                    schedule.lessons = new List<Lesson>();
                    schedule.Day = reader.GetDateTime(2);
                    schedule.Week_number = reader.GetInt32(1);
                    Lesson lesson = new Lesson();
                    lesson.Id = reader.GetInt32(0);
                    lesson.Num = reader.GetInt32(7);
                    lesson.Group = reader.GetString(3);
                    lesson.Subject = reader.GetString(4);
                    lesson.Lecture = reader.GetBoolean(8);
                    lesson.Class = reader.GetValue(5).ToString();
                    lesson.Building = reader.GetString(6);
                    lesson.program = reader.IsDBNull(9) ? null : reader.GetInt32(9);
                    int i = 0;
                    bool flag = false;
                    bool flag1 = false;
                    bool idflag = false;
                    int gindex = 0;
                    int index = 0;
                    int j = 0;
                    int i_d = 0;
                    foreach (var item in schedules)
                    {
                        flag1 = false;
                        idflag = false;
                        var temp = item.lessons;
                        if (item.Day == schedule.Day)
                        {
                            foreach (var les in temp)
                            {
                                if (les.Id != lesson.Id && les.Num == lesson.Num && les.Subject == lesson.Subject && les.Teacher == lesson.Teacher && les.Group != lesson.Group)
                                {
                                    flag1 = true;
                                    gindex = j;
                                    break;
                                }
                                else if (les.Id == lesson.Id)
                                {
                                    idflag = true;
                                    i_d = j;
                                }
                                j++;
                            }
                            flag = true;
                            index = i;
                            break;
                        }
                        i++;
                    }
                    if (flag == true && flag1 == false && idflag == false)
                    {
                        schedules[index].lessons.Add(lesson);
                    }
                    else if (flag == true && flag1 == true && idflag == false)
                    {
                        schedules[index].lessons[gindex].Group += ", " + lesson.Group;
                    }
                    else if (idflag == true)
                    {
                        schedules[index].lessons[i_d].program = lesson.program;
                    }
                    else if (flag == false)
                    {
                        schedule.lessons.Add(lesson);
                        schedules.Add(schedule);
                    }
                }
                connection.Close();
                JsonSerializerOptions options = new()
                {

                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize<List<Schedule>>(schedules, options);
                return jsonString;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
