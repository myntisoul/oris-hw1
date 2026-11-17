using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oris2
{
    public class Size
    {
        public int Height {  get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public Size(int height, int width, int length)
        {
            Height = height;
            Width = width;
            Length = length;
        }
    }
}
