using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class UserItem
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Pictureurl { get; set; }
    }
}
