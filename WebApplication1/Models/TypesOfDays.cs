using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class TypesOfDays
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid HotelId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Hotel? Hotel { get; set; }

    }
}