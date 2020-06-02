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
    public partial class Form_Paint : Form
    {
        public DimText DimTextForm { get; set; } = new DimText();
        public Autodesk.AutoCAD.Colors.Color DimColor { get; set; }
        public string DimType { get; set; }
        public Form_Paint()
        {
            InitializeComponent();
        }

        private void Form_Paint_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DimType = comboBox2.GetItemText(comboBox2.SelectedItem);
            DimColor = Class1.GetColor(comboBox1);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (DimTextForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }
    }
}
