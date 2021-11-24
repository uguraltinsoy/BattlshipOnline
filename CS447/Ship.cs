using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS447
{
    class Ship
    {
        
        public string name { get; set; }
        public char letter { get; set; }
        public int length { get; set; }

        public Ship(string name, char letter, int length)
        {
            this.name = name;
            this.letter = letter;
            this.length = length;
        }
    }
}
