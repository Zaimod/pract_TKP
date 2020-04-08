using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Drawing;
using System.Windows;

[assembly: CommandClass(typeof(pr6new.Class1))]
namespace pr6new
{
    public class Class1
    {
        //Lab 5A Hello World
        [CommandMethod("HelloWorld")] // назва команди в Autocad
        public void HelloWorld()
        {
            //редактор
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Hello World!");
        }

        //Lab 5B: "User Input" and Lab 6A: "Database: Create the circle and the Block"
        [CommandMethod("addAnEnt")]
        public void AddAnEnt()
        {
            //редактор
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            //запит на введення користувачем дії
            PromptKeywordOptions getWhichEntityOptions = new PromptKeywordOptions("Which entity do you want to create? [Circle/Block] : ", "Circle Block");

            //результат
            PromptResult getWhichEntityResult = ed.GetKeywords(getWhichEntityOptions); //передача управління клавіатурі

            if (getWhichEntityResult.Status == PromptStatus.OK)//Перевірка
            {
                switch (getWhichEntityResult.StringResult)
                {
                    case "Circle":
                        PromptPointOptions getPointOptions = new PromptPointOptions("Pick Center Point: ");

                        PromptPointResult getPointResult = ed.GetPoint(getPointOptions);

                        if (getPointResult.Status == PromptStatus.OK)
                        {
                            PromptDistanceOptions getRadiusOptions = new PromptDistanceOptions("Pick Radius: ");

                            getRadiusOptions.BasePoint = getPointResult.Value;

                            getRadiusOptions.UseBasePoint = true;

                            PromptDoubleResult getRadiusResult = ed.GetDistance(getRadiusOptions);

                            if (getRadiusResult.Status == PromptStatus.OK)
                            {
                                //База даних поточного чертежа
                                Database dwg = ed.Document.Database;

                                //Транзацкії для роботи з примітивами 
                                Transaction trans = dwg.TransactionManager.StartTransaction();

                                try
                                {
                                    Circle circle = new Circle(getPointResult.Value, Vector3d.ZAxis, getRadiusResult.Value);
                                    //получаємо поточне середовище листа
                                    BlockTableRecord btr = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                                    btr.AppendEntity(circle);

                                    //добавляє примітив на лист                             
                                    trans.AddNewlyCreatedDBObject(circle, true);
                                    //Завершення транзакції
                                    trans.Commit();
                                }
                                catch (System.Exception ex) ///?
                                {
                                    ed.WriteMessage("Problem due to " + ex.Message.ToString());
                                }
                                finally
                                {
                                    trans.Dispose();
                                }
                            }
                        }
                        break;
                    //теж саме тільки робить копію
                    case "Block":
                        PromptStringOptions blockNameOptions = new PromptStringOptions("Enter the name of the block to create: ");

                        blockNameOptions.AllowSpaces = false;

                        PromptResult blockNameResult = ed.GetString(blockNameOptions);

                        if (blockNameResult.Status == PromptStatus.OK)
                        {
                            Database dwg = ed.Document.Database;

                            Transaction trans = dwg.TransactionManager.StartTransaction();

                            try
                            {
                                BlockTableRecord newBlockDef = new BlockTableRecord();
                                newBlockDef.Name = blockNameResult.StringResult;

                                BlockTable blockTable = trans.GetObject(dwg.BlockTableId, OpenMode.ForRead) as BlockTable;

                                if (blockTable.Has(blockNameResult.StringResult) == false)
                                {
                                    blockTable.UpgradeOpen();
                                    blockTable.Add(newBlockDef);

                                    trans.AddNewlyCreatedDBObject(newBlockDef, true);

                                    Circle circle1 = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 10);
                                    newBlockDef.AppendEntity(circle1);

                                    Circle circle2 = new Circle(new Point3d(20, 10, 0), Vector3d.ZAxis, 10); //зміщення на x=20 y=10
                                    newBlockDef.AppendEntity(circle2);

                                    trans.AddNewlyCreatedDBObject(circle1, true);
                                    trans.AddNewlyCreatedDBObject(circle2, true);

                                    PromptPointOptions blockRefPointOptions = new PromptPointOptions("Pick Insertion point of BlockRef: ");
                                    PromptPointResult blockRefPointResult = ed.GetPoint(blockRefPointOptions);

                                    if (blockRefPointResult.Status != PromptStatus.OK)
                                    {
                                        trans.Dispose();

                                        return;
                                    }

                                    BlockReference blockRef = new BlockReference(blockRefPointResult.Value, newBlockDef.ObjectId);

                                    BlockTableRecord curSpace = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                                    curSpace.AppendEntity(blockRef);

                                    trans.AddNewlyCreatedDBObject(blockRef, true);
                                    trans.Commit();
                                }
                            }
                            catch (System.Exception ex)
                            {
                                ed.WriteMessage("Problem occured because " + ex.Message.ToString());
                            }
                            finally
                            {
                                trans.Dispose();
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
        //Lab 6B: PaletteSet and Database Events
        public PaletteSet myPaletteSet;
        public UserControl1 myPalette;

        [CommandMethod("Pallete")]
        public void pallete()
        {
            if (myPaletteSet == null)
            {
                myPaletteSet = new PaletteSet("My Pallete", new Guid("0BD2504F-9F4E-4E35-8D81-5BED6D07C230"));

                myPalette = new UserControl1();
                myPaletteSet.Add("Pallete1", myPalette);
            }
            myPaletteSet.Visible = true;
        }
        //Continue Lab 6B
        [CommandMethod("AddDbEvents")]
        public void addDbEvents()
        {
            if (myPalette == null)
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

                ed.WriteMessage("\r" + "Please run the 'palette command first");

            }
            Database curDwg = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Database;

            curDwg.ObjectAppended += new ObjectEventHandler(callback_objectAppended);

            curDwg.ObjectErased += new ObjectErasedEventHandler(callback_objectErased);

            curDwg.ObjectReappended += new ObjectEventHandler(callback_objectReappended);

            curDwg.ObjectUnappended += new ObjectEventHandler(callback_objectUnappended);
        }
        //добавляє запис у palette
        private void callback_objectAppended(object sender, ObjectEventArgs e)
        {
            TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());
            newNode.Tag = e.DBObject.ObjectId.ToString();


        }
        //видаляє запис
        private void callback_objectErased(object sender, ObjectErasedEventArgs e)
        {

            if (e.Erased)
            {
                foreach (TreeNode node in myPalette.treeView1.Nodes)
                {
                    if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                    {
                        node.Remove();
                        break;
                    }
                }
            }
            else
            {
                TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());
                newNode.Tag = e.DBObject.ObjectId.ToString();
            }
        }
        //повторно додає запис
        private void callback_objectReappended(object sender, ObjectEventArgs e)
        {
            TreeNode newNode = myPalette.treeView1.Nodes.Add(e.DBObject.GetType().ToString());
            newNode.Tag = e.DBObject.ObjectId.ToString();
        }
        //видаляє
        private void callback_objectUnappended(object sender, ObjectEventArgs e)
        {
            foreach (TreeNode node in myPalette.treeView1.Nodes)
            {
                if (node.Tag.ToString() == e.DBObject.ObjectId.ToString())
                {
                    node.Remove();
                    break;
                }
            }
        }

        //pr7
        [CommandMethod("addData")]
        public void addData()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor; //Редактор
            PromptEntityResult getEntityResult = ed.GetEntity("Pick an entity to add an Extension Dictionary to : "); //Отримання вводу користувача

            if (getEntityResult.Status == PromptStatus.OK)
            {
                Transaction trans = ed.Document.TransactionManager.StartTransaction();
                try
                {
                    Entity ent = trans.GetObject(getEntityResult.ObjectId, OpenMode.ForRead) as Entity;
                    if (ent.ExtensionDictionary.IsNull)
                    {
                        ent.UpgradeOpen();
                        ent.CreateExtensionDictionary();// створення словника
                    }

                    DBDictionary extensionDict = trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                    if (extensionDict.Contains("MyData"))
                    {
                        ObjectId entryId = extensionDict.GetAt("MyData");
                        ed.WriteMessage("\n" + "This entity already has data...");

                        Xrecord myXrecord = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;

                        foreach (TypedValue value in myXrecord.Data)
                        {
                            ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());
                        }
                    }
                    else
                    {
                        extensionDict.UpgradeOpen();

                        Xrecord myXrecord = new Xrecord();

                        ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                            new TypedValue((int)DxfCode.Text, "MyStockData"),
                            new TypedValue((int)DxfCode.Real, 51.9),
                            new TypedValue((int)DxfCode.Real, 100.0),
                            new TypedValue((int)DxfCode.Real, 320.6));

                        myXrecord.Data = data;

                        extensionDict.SetAt("MyData", myXrecord);

                        trans.AddNewlyCreatedDBObject(myXrecord, true);

                        if (myPalette != null)
                        {
                            foreach (TreeNode node in myPalette.treeView1.Nodes)
                            {
                                if (node.Tag.ToString() == ent.ObjectId.ToString())
                                {
                                    TreeNode childnode = node.Nodes.Add("Extension dictionary");

                                    foreach (TypedValue value in myXrecord.Data)
                                    {
                                        childnode.Nodes.Add(value.ToString());
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        //continue pr7
        [CommandMethod("addDataToNOD")]
        public void addDataToNOD()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();

            try
            {
                DBDictionary nod = trans.GetObject(ed.Document.Database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;

                if (nod.Contains("MyData"))
                {
                    ObjectId entryId = nod.GetAt("MyData");
                    ed.WriteMessage("\n" + "The NOD already has our data...");

                    Xrecord myXrecord = new Xrecord();
                    myXrecord = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;

                    foreach (TypedValue value in myXrecord.Data)
                    {
                        ed.WriteMessage("\n" + value.TypeCode.ToString() + " . " + value.Value.ToString());
                    }
                }
                else
                {
                    nod.UpgradeOpen();

                    Xrecord myXrecord = new Xrecord();

                    ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Int16, 1),
                        new TypedValue((int)DxfCode.Text, "MyCompanyDefaultSettings"),
                        new TypedValue((int)DxfCode.Real, 51.9),
                        new TypedValue((int)DxfCode.Real, 100.0),
                        new TypedValue((int)DxfCode.Real, 320.6));

                    myXrecord.Data = data;

                    nod.SetAt("MyData", myXrecord);
                    trans.AddNewlyCreatedDBObject(myXrecord, true);
                }
                trans.Commit();

            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("a problem occured because " + ex.Message);

            }
            finally
            {
                trans.Dispose();
            }
        }

        //Lab 8 – PointMonitor
        [CommandMethod("addPointMonitor")]
        public void startMonitor()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.PointMonitor += new PointMonitorEventHandler(MyPointMonitor);
        }

        public void MyPointMonitor(object sender, PointMonitorEventArgs e)
        {
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();

            if (fullEntPath.Length != 0)
            {
                Transaction trans = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

                try
                {
                    Entity ent = trans.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead) as Entity;

                    e.AppendToolTipText("the Entity is a " + ent.GetType().ToString());

                    if (myPalette == null)
                    {
                        return;
                    }

                    System.Drawing.Font fontRegular = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Regular);
                    System.Drawing.Font fontBold = new System.Drawing.Font("Microsoft Sans Serif", 8, System.Drawing.FontStyle.Bold);

                    myPalette.treeView1.SuspendLayout();

                    foreach (TreeNode node in myPalette.treeView1.Nodes)
                    {
                        if (node.Tag.ToString() == ent.ObjectId.ToString())
                        {
                            if (!fontBold.Equals(node.NodeFont))
                            {
                                node.NodeFont = fontBold;
                                node.Text = node.Text;
                            }
                        }
                        else
                        {
                            if (!fontRegular.Equals(node.NodeFont))
                            {
                                node.NodeFont = fontRegular;
                            }
                        }

                    }
                    myPalette.treeView1.ResumeLayout();
                    myPalette.treeView1.Refresh();
                    myPalette.treeView1.Update();

                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        [CommandMethod("newInput")]
        public void NewInput()
        {
            UIElement element = new UIElement();
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ed.PointMonitor += new PointMonitorEventHandler(MyInputMonitor);

            ed.TurnForcedPickOn();

            PromptPointOptions getPointOptions = new PromptPointOptions("Pick a Point: ");
            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);

            ed.GetPoint(getPointOptions);

            ed.PointMonitor -= new PointMonitorEventHandler(MyInputMonitor);

        }

        public void MyInputMonitor(Object sender, PointMonitorEventArgs e)
        {
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();

            if (fullEntPath.Length != 0)
            {
                Transaction trans = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();

                try
                {
                    Curve ent = trans.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead) as Curve;

                    if (ent.ExtensionDictionary.IsValid)
                    {
                        DBDictionary extensionDict = trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead) as DBDictionary;
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        Xrecord myXrecord = trans.GetObject(entryId, OpenMode.ForRead) as Xrecord;

                        foreach (TypedValue myTypedVal in myXrecord.Data)
                        {
                            if (myTypedVal.TypeCode == (int)DxfCode.Real)
                            {
                                Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);

                                Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);

                                double xDist = 10 / pixels.X;
                                double yDist = 10 / pixels.Y;

                                Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);

                                e.Context.DrawContext.Geometry.Draw(circle);

                                DBText text = new DBText();
                                text.SetDatabaseDefaults();

                                text.Position = pnt + new Vector3d(xDist, yDist, 0);
                                text.TextString = myTypedVal.Value.ToString();

                                text.Height = yDist;

                                e.Context.DrawContext.Geometry.Draw(text);
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.ToString());
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}