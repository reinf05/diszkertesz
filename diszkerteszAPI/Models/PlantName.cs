using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diszkerteszAPI.Models
{
    [Table("PlantNames")]
    public class PlantName
    {
        public int Id { get; set; }
        public int TranslateId { get; set; }
        public string Name { get; set; }

        [MaxLength(2)]
        public string Language { get; set; }

        [ForeignKey("TranslateId")]
        public Translate Translate { get; set; }
    }
}
