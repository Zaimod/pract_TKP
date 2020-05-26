using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pr6new
{
    public partial class Lab13 : Form
    {
        Class1 class1 = new Class1();
        public Lab13()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {         
            class1.lab13_go(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox3.Text), Convert.ToDouble(textBox4.Text), Convert.ToDouble(textBox5.Text), Convert.ToDouble(textBox8.Text), Convert.ToInt32(comboBox3.SelectedIndex), Convert.ToInt32(comboBox1.SelectedIndex));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
