namespace Domain.Models.Entities
{
    public class ActionType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Activity> Activities { get; set; } = [];
    }
}