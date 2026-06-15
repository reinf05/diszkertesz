using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace diszkerteszAPI.Models
{
    [PrimaryKey(nameof(PathogensId))]
    public class Pathogen
    {
        public int PathogensId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Defense { get; set; }

        [JsonIgnore]
        public ICollection<Detail> Details { get; set; } = new List<Detail>();
    }
}
