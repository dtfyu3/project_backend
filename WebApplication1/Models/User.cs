using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class User
    {
        public int? Id { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Surname { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string LastName { get; set; }
        public string Surname_Name {  get; set; }
        public bool? Type {  get; set; }
        public  int? Group { get; set; }
    }
}
