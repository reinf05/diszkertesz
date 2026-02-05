using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace diszkerteszAPI.Models
{
    public class UsersShared
    {
        [Key]
        public int Id { get; set; }
        public int Owner { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Pictureurl { get; set; }
        public int? PerenualID { get; set; }
        public string? LatinName { get; set; }
        public PlantTips? Tips { get; set; }

    }
}
