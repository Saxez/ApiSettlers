using Microsoft.Identity.Client;
using System.Text.RegularExpressions;

namespace Project1.Models
{
    public class Record
    {
        public Guid Id { get; set; }

        public int Price { get; set; }

        public int Capacity { get; set; }

        public int Count { get; set; }
        public DateTime DateOfCheckIn { get; set; }

        public DateTime DateOfCheckOut { get; set; }

        public string Name { get; set; }
        public Guid HotelId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Hotel? Hotel { get; set; }

        public Guid GroupId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Groups? Group { get; set; }
    }
}