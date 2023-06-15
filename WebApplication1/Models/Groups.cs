using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class Groups
    {
        [Key]
        public Guid Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public DateTime DateOfStart { get; set; }
        public DateTime DateOfEnd { get; set; }
        public bool Status { get; set; }
        public int PreferredType { get; set; }
        public Nullable<Guid> ManagerId { get; set; }
        public Users? Manager { get; set; }
        public Guid MassEventId { get; set; } // внешний ключ
        public MassEvents? MassEvent { get; set; } // навигационное свойство
        public List<Records> Records { get; set; } = new();
        public List<Settlers> Settlers { get; set; }
    }
}