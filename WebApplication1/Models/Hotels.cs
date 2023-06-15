using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class Hotels
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
        public MassEvents? MassEvent { get; set; } // навигационное свойство

        [System.Text.Json.Serialization.JsonIgnore]
        public Nullable<Guid> HotelUserId { get; set; } // внешний ключ
        [System.Text.Json.Serialization.JsonIgnore]
        public Users? HotelUser { get; set; }


        public List<EnteredDataHotels> EnteredDataHotels { get; set; } = new();

        public List<RecordDataHotels> RecordDataHotels { get; set; } = new();

        public List<DifferenceDataHotels> DifferenceDataHotels { get; set; } = new();

        public List<UserXHotels> UserXHotels { get; set; } = new();

        public List<Records> Records { get; set; } = new();

        public List<Settlers> Settlers { get; set; } = new();
    }
}