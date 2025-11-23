using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diszkerteszAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Meeid { get; set; }
        public bool Provisioned { get; set; }
    }
}
