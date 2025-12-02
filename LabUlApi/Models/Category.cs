namespace LabUlApi.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public List<Dish> Dishes { get; set; } = new();
    }
}