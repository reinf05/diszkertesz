using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class Quiz
    {
        public List<string> ImagePath { get; set; }
        public string[] Names { get; set; } = new string[4];
        public string ChosenImage { get; set; }
        public string Correct { get; set; }
    }
}
