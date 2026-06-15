namespace diszkerteszAPI.Models
{
    public class Fullplant
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Namel { get; set; }
        public string Nameh { get; set; }
        public List<string> Imagepath { get; set; }
        public string Description { get; set; }
        public string Leaf { get; set; }
        public string Flower { get; set; }
        public string Origin { get; set; }
        public string Light { get; set; }
        public string Soil { get; set; }
        public string Water { get; set; }
        public string Usage { get; set; }
        public string Defense { get; set; }
        public List<Pathogen> Pathogens { get; set; }
        public string Propagation { get; set; }
        public string Species { get; set; }
    }
}
