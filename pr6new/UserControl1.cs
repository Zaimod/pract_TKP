using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using static pr6new.Class1;

namespace pr6new
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void DragMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons == MouseButtons.Left)
            {
                DoDragDrop(new MyDropTarget(), DragDropEffects.All);
            }
        }

 
    }
    public class MyDropTarget : DropTarget
    {
        public override void OnDrop(DragEventArgs e)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            //adskClass adsk = new adskClass();
            try
            {
                using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.LockDocument()) 
                {

                    //adsk.AddAnEnt();
                }
            }
            catch (Exception ex)
            {
                ed.WriteMessage("Error Handling OnDrop" + ex.Message);
            }
        }
    }
}
