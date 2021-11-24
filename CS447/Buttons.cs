using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS447
{
    class Buttons
    {
        public Button button { get; set; }
        public bool isFull { get; set; }

        public Buttons(Button button, bool isFull)
        {
            this.button = button;
            this.isFull = isFull;
        }
    }
}
