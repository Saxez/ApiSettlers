using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class UserXHotel
    {
        [Key]
        public Guid Id { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Guid HotelId { get; set; }

        public User? User { get; set; }
        public Hotel? Hotel { get; set; }
    }
}