using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class Hotel
    {
        [Key]
        public Guid Id { get; set; }


        public string? Name { get; set; }

        public string? Adress { get; set; }

        public string? CancelCondition { get; set; }

        public string? CheckIn { get; set; }

        public string? CheckOut { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Link { get; set; }

        public int Stars { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Guid MassEventId { get; set; } // внешний ключ
        [System.Text.Json.Serialization.JsonIgnore]
        public MassEvent? MassEvent { get; set; } // навигационное свойство

        [System.Text.Json.Serialization.JsonIgnore]
        public Nullable<Guid> HotelUserId { get; set; } // внешний ключ
        [System.Text.Json.Serialization.JsonIgnore]
        public User? HotelUser { get; set; }


        public List<EnteredDataHotel> EnteredDataHotels { get; set; } = new();

        public List<RecordDataHotel> RecordDataHotels { get; set; } = new();

        public List<DifferenceDataHotel> DifferenceDataHotels { get; set; } = new();

        public List<UserXHotel> UserXHotels { get; set; } = new();

        public List<Record> Records { get; set; } = new();

        public List<Settler> Settlers { get; set; } = new();
    }
}