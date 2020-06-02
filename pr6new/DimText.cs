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

    public partial class DimText : Form
    {
        public double? TextHeight { get; set; }
        public double? TextRotation { get; set; }
        public Autodesk.AutoCAD.Colors.Color TextColor { get; set; }
        public DimText()
        {
            InitializeComponent();
        }
        private Autodesk.AutoCAD.Colors.Color GetColor()
        {
            if (this.ColorComboBox.GetItemText(this.ColorComboBox.SelectedItem) == "Red")
            {
                return Autodesk.AutoCAD.Colors.Color.FromRgb(200, 0, 0);
            }

            if (this.ColorComboBox.GetItemText(this.ColorComboBox.SelectedItem) == "Green")
            {
                return Autodesk.AutoCAD.Colors.Color.FromRgb(0, 200, 0);
            }

            if (this.ColorComboBox.GetItemText(this.ColorComboBox.SelectedItem) == "Blue")
            {
                return Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 200);
            }

            else
            {
                return Autodesk.AutoCAD.Colors.Color.FromRgb(0, 0, 0);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (double.TryParse(HeightTextBox.Text, out double height))
            {
                TextHeight = height;
            }

            if (double.TryParse(RotationTextBox.Text, out double rotation))
            {
                TextRotation = rotation;
            }

            if (ColorComboBox.SelectedIndex != -1)
            {
                TextColor = GetColor();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
