using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            BringToFront();
            InitializeComponent();
        }

        public int f2Timer
        {
            get { return (int)numericUpDownCount.Value; }

            set { numericUpDownCount.Value = value; }

        }

        public int f2Width
        {
            get { return (int)numericUpDownWidth.Value; }
            set { numericUpDownWidth.Value = value; }
        }

        public int f2Height
        {
            get { return (int)numericUpDownHeight.Value; }
            set { numericUpDownHeight.Value = value; }
        }
    }
}
