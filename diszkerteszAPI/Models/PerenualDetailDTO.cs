namespace diszkerteszAPI.Models
{
    public class PerenualDetailDTO
    {
        public int Id { get; set; }
        public string Watering { get; set; }
        public List<string>? Sunlight { get; set; }
        public List<string>? Soil { get; set; }
        public string Care_level { get; set; }
        public string Cycle { get; set; }
        public bool Poisonous_to_humans { get; set; }
        public bool Poisonous_to_pets { get; set; }
    }
}
