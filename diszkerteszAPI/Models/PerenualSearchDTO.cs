namespace diszkerteszAPI.Models
{
    public class PerenualSearchDTO
    {
        public List<Species> Data { get; set; }

    }

    public class Species
    {
        public int Id { get; set; }
        public List<string> Scientific_Name { get; set; }
        public List<string> Other_name { get; set; }
    }
}
