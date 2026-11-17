using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oris2
{
    public class MadeIn
    {
        public int Code { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public MadeIn(int code, string country, string city)
        {
            Code = code;
            Country = country;
            City = city;
        }
    }
}
