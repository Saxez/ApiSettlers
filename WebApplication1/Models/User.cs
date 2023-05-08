using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        private List<UserXHotel> UserXHotels { get; set; } = new();
        private List<Groups> Groups { get; set; } = new();


    }


}