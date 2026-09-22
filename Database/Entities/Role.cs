namespace Database.Entities
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<User> NavUsers { get; set; } = new List<User>();
    }
}
