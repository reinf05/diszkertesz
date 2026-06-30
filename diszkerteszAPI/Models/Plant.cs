using System.ComponentModel.DataAnnotations.Schema;

namespace diszkerteszAPI.Models
{
    public class Plant
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Namel { get; set; }
        public string Nameh { get; set; }
        public string Imagepath { get; set; }
        [NotMapped]
        public List<string> Images { get; set; }
    }
}
