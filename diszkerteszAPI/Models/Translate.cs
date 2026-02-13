using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diszkerteszAPI.Models
{
    [Table("Translate")]
    public class Translate
    {
        [Key]
        public int Id { get; set; }
        public int PerenualID { get; set; }
        public string LatinName { get; set; }
        public ICollection<PlantName> Names { get; set; }
        public PlantTips Tips { get; set; }
    }
}
