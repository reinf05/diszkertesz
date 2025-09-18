using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class Quiz
    {
        public string ImagePath { get; set; }
        public string[] Names { get; set; } = new string[4];
    }
}
