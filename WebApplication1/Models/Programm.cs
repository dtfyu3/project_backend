using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class Programm
    {
        public int Id { get; set; }
        public int teacher_id { get; set; }
        public int assistent {  get; set; }
        public string faculty { get; set; }
        public int course {  get; set; }
        public int sem {  get; set; }
        public string group { get; set; }
        public int? labsCount { get; set; }
        public string subject {  get; set; }
        public List<Lab>? labs { get; set; }
    }
    public class Subject
    {
        public string subject { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Programm>? programms { get; set;}

        public string? teacher { get; set; }
    }
}
