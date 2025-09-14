using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;

namespace diszkerteszAPI.Models
{
    [PrimaryKey(nameof(Plant_ID))]
    public class Detail
    {
        public int Plant_ID { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Pathogens { get; set; }
        public string Propagation { get; set; }
    }
}
