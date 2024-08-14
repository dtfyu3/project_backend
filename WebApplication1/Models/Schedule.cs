using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Schedule
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? Id { get; set; }
        public int Week_number { get; set; }
        public DateTime Day { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Group { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Teacher { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Subject { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Class { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Building { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Number { get; set; }
        public List<Lesson> lessons { get; set; }
    }
    public class Lesson
    {
        public int? Id { get; set; }
        public int Num { get; set; }
        public string Subject { get; set; }
        public bool? Lecture { get; set; }
        public string Teacher { get; set; }
        public int Teacher_id {  get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Group { get; set; }
        public string Class { get; set; }
        public string Building { get; set; }
        public int? program { get; set; }
    }
}
