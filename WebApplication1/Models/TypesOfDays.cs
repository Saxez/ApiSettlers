using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class TypesOfDays
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid HotelId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Hotels? Hotel { get; set; }

    }
}