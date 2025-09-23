using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Models
{
    public class IdentificationResult
    {
        public List<Result> results { get; set; }
    }

    public class Result
    {
        public double score { get; set; }
        public Species species { get; set; }
    }

    public class Species
    {
        public string scientificNameWithoutAuthor { get; set; }
        public List<string> commonNames { get; set; }
    }

    public class IdentificationShow
    {
        public double Percent { get; set; }
        public string Scientific { get; set; }
        public List<string> CommonNames { get; set; }
    }
}
