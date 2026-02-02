using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class FullPlant
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Namel { get; set; }
        public string Nameh { get; set; }
        public List<string> Imagepath { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Pathogens { get; set; }
        public string Propagation { get; set; }
    }
}
