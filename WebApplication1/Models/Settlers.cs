using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Project1.Models
{
    public class Settlers
    {
        [Key]
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public string Contact { get; set; }


        [System.Text.Json.Serialization.JsonIgnore]
        public Guid GroupsId { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Groups? Groups { get; set; }


        [System.Text.Json.Serialization.JsonIgnore]
        public Nullable<Guid> HotelId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Hotels? Hotel { get; set; }

    }
}