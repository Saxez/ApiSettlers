using System;
using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class UserXHotels
    {

        public Guid UserId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Guid HotelId { get; set; }

        public Users? User { get; set; }
        public Hotels? Hotel { get; set; }
    }
}