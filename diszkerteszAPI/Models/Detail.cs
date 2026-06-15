using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace diszkerteszAPI.Models
{
    [PrimaryKey(nameof(Plant_ID))]
    public class Detail
    {
        public int Plant_ID { get; set; }
        public string? Description { get; set; }
        public string? Leaf { get; set; }
        public string? Flower { get; set; }
        public string? Origin { get; set; }
        public string? Light { get; set; }
        public string? Soil { get; set; }
        public string? Water { get; set; }
        public string? Usage { get; set; }
        public string? Defense { get; set; }
        public string? Propagation { get; set; }
        public string? Species { get; set; }

        [JsonIgnore]
        public ICollection<Pathogen> Pathogens { get; set; } = new List<Pathogen>();
    }
}
