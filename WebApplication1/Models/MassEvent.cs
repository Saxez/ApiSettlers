using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class MassEvent
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime DateOfStart { get; set; }

        public DateTime DateOfEnd { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public List<Hotel> Hotels { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public List<Groups> Groups { get; set; } = new();


    }
}