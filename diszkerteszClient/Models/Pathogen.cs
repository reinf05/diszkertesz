using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class Pathogen
    {
        public int PathogensId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Defense { get; set; }
    }
}
