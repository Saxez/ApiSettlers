using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class RecordDataHotel
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateIn { get; set; }
        public int Capacity { get; set; }
        public int Price { get; set; }
        public int Count { get; set; }
        public int Type { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid HotelId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Hotel? Hotel { get; set; }
    }
}