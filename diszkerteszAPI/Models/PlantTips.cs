namespace diszkerteszAPI.Models
{
    public class PlantTips
    {
        public string Water { get; set; }
        public string Light { get; set; }
        public string? Soil { get; set; }
        public string Cycle { get; set; }
        public string CareLevel { get; set; }
        public string Poisonous { get; set; }
        public string? LatinName { get; set; }
        public List<string>? HungarianName { get; set; }

    }
}
