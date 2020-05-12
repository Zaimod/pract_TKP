﻿using System;
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
using Autodesk.AutoCAD.GraphicsInterface;

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
                                catch (System.Exception ex)
                                {
                                    ed.WriteMessage("Problem due to " + ex.Message.ToString());
                                }
                                finally
                                {
                                    trans.Dispose(); //cборщик мусора
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
            PromptEntityResult getEntityResult = ed.GetEntity("Pick an entity to add an Extension Dictionary to : ");

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
                    if (extensionDict.Contains("MyData"))// перевірка на існуючий
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
        //Lab 9 – EntityJig  
        public class MyCircleJig : EntityJig
        {
            Point3d centerPoint = new Point3d();
            double radius = 0.0;

            public int CurrentInputValue;

            public MyCircleJig(Entity ent) : base(ent) { CurrentInputValue = 0; }

            protected override SamplerStatus Sampler(JigPrompts prompts)
            {
                switch (CurrentInputValue)
                {
                    case 0:
                        Point3d oldPnt = centerPoint;
                        PromptPointResult jigPromptResult = prompts.AcquirePoint("Pick center point: ");

                        if (jigPromptResult.Status == PromptStatus.OK)
                        {
                            centerPoint = jigPromptResult.Value;

                            if (oldPnt.DistanceTo(centerPoint) < 0.0001)
                            {
                                return SamplerStatus.NoChange;
                            }
                        }
                        return SamplerStatus.OK;

                    case 1:
                        double oldRadius = radius;
                        JigPromptDistanceOptions jigPromptDistanceOptions = new JigPromptDistanceOptions("Pick radius: ");

                        jigPromptDistanceOptions.UseBasePoint = true;
                        jigPromptDistanceOptions.BasePoint = centerPoint;

                        PromptDoubleResult jigPromptPointResult = prompts.AcquireDistance(jigPromptDistanceOptions);

                        if (jigPromptPointResult.Status == PromptStatus.OK)
                        {
                            radius = jigPromptPointResult.Value;

                            if (Math.Abs(radius) < 0.1)
                            {
                                radius = 1;
                            }

                            if (Math.Abs(oldRadius - radius) < 0.0001)
                            {
                                return SamplerStatus.NoChange;
                            }
                        }
                        return SamplerStatus.OK;

                    default:
                        return 0;
                }
            }

            protected override bool Update()
            {
                switch (CurrentInputValue)
                {
                    case 0:
                        ((Circle)Entity).Center = centerPoint;
                        break;

                    case 1:
                        ((Circle)Entity).Radius = radius;
                        break;
                }
                return true;
            }

        }

        [CommandMethod("circleJig")]
        public void CircleJig()
        {
            Circle circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 10);

            MyCircleJig jig = new MyCircleJig(circle);

            for (int i = 0; i <= 1; i++)
            {
                jig.CurrentInputValue = i;

                Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                PromptResult promptResult = ed.Drag(jig);

                if (promptResult.Status == PromptStatus.Cancel || promptResult.Status == PromptStatus.Error)
                {
                    return;
                }
            }

            Database dwg = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Database;
            Transaction trans = dwg.TransactionManager.StartTransaction();

            try
            {
                BlockTableRecord currentSpace = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                currentSpace.AppendEntity(circle);

                trans.AddNewlyCreatedDBObject(circle, true);

                trans.Commit();
            }
            catch (System.Exception) { }
            finally
            {
                trans.Dispose();
            }
        }

        //Lab 11
        [CommandMethod("point")]
        public void point()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions getXOptions = new PromptPointOptions("Pick X and Y: ");

            PromptPointResult getXResult = ed.GetPoint(getXOptions);

            Database dwg = ed.Document.Database;

            Transaction trans = dwg.TransactionManager.StartTransaction();

            try
            {
                DBPoint point = new DBPoint(new Point3d(getXResult.Value.X, getXResult.Value.Y, 0));                

                BlockTableRecord btr = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(point);

                dwg.Pdmode = 32;
                //добавляє примітив на лист                             
                trans.AddNewlyCreatedDBObject(point, true);
                //Завершення транзакції
                trans.Commit();
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

        [CommandMethod("line")]
        public void line()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions getBeginOptions = new PromptPointOptions("The begin of the line: ");

            PromptPointResult getBeginResult = ed.GetPoint(getBeginOptions);

            PromptPointOptions getEndOptions = new PromptPointOptions("The end of the line: ");

            PromptPointResult getEndResult = ed.GetPoint(getEndOptions);

            Database dwg = ed.Document.Database;

            Transaction trans = dwg.TransactionManager.StartTransaction();

            try
            {
                Line line = new Line(new Point3d(getBeginResult.Value.X, getBeginResult.Value.Y, 0), new Point3d(getEndResult.Value.X, getEndResult.Value.Y, 0));

                BlockTableRecord btr = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(line);

                //добавляє примітив на лист                             
                trans.AddNewlyCreatedDBObject(line, true);
                //Завершення транзакції
                trans.Commit();
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

        [CommandMethod("pline")]
        public void pline()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions getBeginOptions = new PromptPointOptions("The begin of the line: ");

            PromptPointResult getBeginResult = ed.GetPoint(getBeginOptions);

            PromptPointOptions getNextOptions = new PromptPointOptions("The next of the line: ");

            PromptPointResult getNextResult = ed.GetPoint(getNextOptions);

            PromptPointOptions getEndOptions = new PromptPointOptions("The end of the line: ");

            PromptPointResult getEndResult = ed.GetPoint(getEndOptions);

            Database dwg = ed.Document.Database;

            Transaction trans = dwg.TransactionManager.StartTransaction();

            try
            {
                Autodesk.AutoCAD.DatabaseServices.Polyline polyline = new Autodesk.AutoCAD.DatabaseServices.Polyline();

                polyline.AddVertexAt(0, new Point2d(getBeginResult.Value.X, getBeginResult.Value.Y), 0, 0, 0);
                polyline.AddVertexAt(1, new Point2d(getNextResult.Value.X, getNextResult.Value.Y), 0, 0, 0);
                polyline.AddVertexAt(2, new Point2d(getEndResult.Value.X, getEndResult.Value.Y), 0, 0, 0);

                BlockTableRecord btr = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(polyline);

                //добавляє примітив на лист                             
                trans.AddNewlyCreatedDBObject(polyline, true);
                //Завершення транзакції
                trans.Commit();
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

        [CommandMethod("circle_01")]
        public void circle1()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

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
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage("Problem due to " + ex.Message.ToString());
                    }
                    finally
                    {
                        trans.Dispose(); //cборщик мусора
                    }
                }
            }
        }
        [CommandMethod("circle_02")]
        public void circle2()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

            PromptPointOptions getPointOptions = new PromptPointOptions("Pick Center Point: ");

            PromptPointResult getPointResult = ed.GetPoint(getPointOptions);

            Database dwg = ed.Document.Database;

            Vector3d x = dwg.Ucsxdir;
            Vector3d y = dwg.Ucsydir;

            Vector3d normalVec = x.CrossProduct(y);
            Vector3d axisvec = normalVec.GetNormal();

            

            //Транзацкії для роботи з примітивами 
            Transaction trans = dwg.TransactionManager.StartTransaction();

            try
            {
                Ellipse ellipse = new Ellipse(new Point3d(getPointResult.Value.X, getPointResult.Value.Y,0), axisvec, new Vector3d(20, 0, 0), 0.5, 0, Math.PI * 2);

                BlockTableRecord btr = trans.GetObject(dwg.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                btr.AppendEntity(ellipse);

                //добавляє примітив на лист                             
                trans.AddNewlyCreatedDBObject(ellipse, true);
                //Завершення транзакції
                trans.Commit();
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("Problem due to " + ex.Message.ToString());
            }
            finally
            {
                trans.Dispose(); //cборщик мусора
            }

        }
        [CommandMethod("square")]
        public void square()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            Editor ed = doc.Editor;

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(db);

                BlockTableRecord btr = Tx.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Point2d pt = new Point2d(0.0, 0.0);
                ///Квадрат 1
                Autodesk.AutoCAD.DatabaseServices.Polyline plBox = new Autodesk.AutoCAD.DatabaseServices.Polyline(4);
                plBox.Normal = Vector3d.ZAxis;
                plBox.AddVertexAt(0, pt, 0.0, -1.0, -1.0);
                plBox.AddVertexAt(1, new Point2d(pt.X + 5, pt.Y), 0.0, -1.0, -1.0);
                plBox.AddVertexAt(2, new Point2d(pt.X + 5, pt.Y + 5), 0.0, -1.0, -1.0);
                plBox.AddVertexAt(3, new Point2d(pt.X, pt.Y + 5), 0.0, -1.0, -1.0);
                plBox.Closed = true;

                ObjectId pLineId2;
                pLineId2 = btr.AppendEntity(plBox);
                ObjectIdCollection ObjIds1 = new ObjectIdCollection();
                ObjIds1.Add(pLineId2);

                ///Штриховка 1
                Hatch oHatch = new Hatch();
                Vector3d normal = new Vector3d(0.0, 0.0, 1.0);
                oHatch.Normal = normal;
                oHatch.Elevation = 0.0;
                oHatch.PatternScale = 2.0;
                oHatch.SetHatchPattern(HatchPatternType.PreDefined, "ZIGZAG");

                btr.AppendEntity(oHatch);
                Tx.AddNewlyCreatedDBObject(oHatch, true);
                Tx.AddNewlyCreatedDBObject(plBox, true);

                oHatch.Associative = true;
                oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds1);
                oHatch.EvaluateHatch(true);

                Tx.Commit();
            }
        }
        public PaletteSet myPaletteSet2;
        UserControl2 usercontrol2;
        //lab12
        [CommandMethod("lab12")]
        public void lab12()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            Editor ed = doc.Editor;

            if (myPaletteSet2 == null)
            {
                myPaletteSet2 = new PaletteSet("Lab12", new Guid("0BD2504F-9F4E-4E35-8D81-5BED6D07C230"));
                usercontrol2 = new UserControl2();

                myPaletteSet2.Add("form1", usercontrol2);
            }

            myPaletteSet2.Visible = true;          

            using (Transaction Tx = db.TransactionManager.StartTransaction())
            {
                ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(db);

                BlockTableRecord btr = Tx.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Circle circle = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 2); //коло

                
                Point2d pt = new Point2d(0.0, 0.0);
                ///Квадрат 1
                Autodesk.AutoCAD.DatabaseServices.Polyline plBox = new Autodesk.AutoCAD.DatabaseServices.Polyline(4);
                plBox.Normal = Vector3d.ZAxis;
                plBox.AddVertexAt(0, pt, 0.0, -1.0, -1.0);
                plBox.AddVertexAt(1, new Point2d(pt.X + 1, pt.Y), 0.0, -1.0, -1.0);
                plBox.AddVertexAt(2, new Point2d(pt.X + 1, pt.Y + 1), 0.0, -1.0, -1.0);
                plBox.AddVertexAt(3, new Point2d(pt.X, pt.Y + 1), 0.0, -1.0, -1.0);
                plBox.Closed = true;
                ////Квадрат2 
                Autodesk.AutoCAD.DatabaseServices.Polyline plBox1 = new Autodesk.AutoCAD.DatabaseServices.Polyline(4);
                plBox1.Normal = Vector3d.ZAxis;
                plBox1.AddVertexAt(0, pt, 0.0, -1.0, -1.0);
                plBox1.AddVertexAt(1, new Point2d(pt.X - 1, pt.Y), 0.0, -1.0, -1.0);
                plBox1.AddVertexAt(2, new Point2d(pt.X - 1, pt.Y - 1), 0.0, -1.0, -1.0);
                plBox1.AddVertexAt(3, new Point2d(pt.X, pt.Y - 1), 0.0, -1.0, -1.0);
                plBox1.Closed = true;
                ///
                ObjectId pLineId;
                pLineId = btr.AppendEntity(circle);
                Tx.AddNewlyCreatedDBObject(circle, true); //додаємо коло на ескіз

                ObjectIdCollection ObjIds = new ObjectIdCollection();
                ObjIds.Add(pLineId);
                ///Штриховка 1
                Hatch oHatch = new Hatch();
                Vector3d normal = new Vector3d(0.0, 0.0, 1.0);
                oHatch.Normal = normal;
                oHatch.Elevation = 0.0;
                oHatch.PatternScale = 2.0;
                oHatch.SetHatchPattern(HatchPatternType.PreDefined, "ZIGZAG");
                oHatch.ColorIndex = 1;
                ////
                btr.AppendEntity(oHatch);
                Tx.AddNewlyCreatedDBObject(oHatch, true);

                ObjectId pLineId2;
                pLineId2 = btr.AppendEntity(plBox);
                ObjectIdCollection ObjIds1 = new ObjectIdCollection();
                ObjIds1.Add(pLineId2);

                ObjectId pLineId3;
                pLineId3 = btr.AppendEntity(plBox1);
                ObjectIdCollection ObjIds2 = new ObjectIdCollection();
                ObjIds2.Add(pLineId3);
                ////Штриховка 2
                Hatch oHatch1 = new Hatch();
                Vector3d normal1 = new Vector3d(0.0, 0.0, 1.0);
                oHatch1.Normal = normal1;
                oHatch1.Elevation = 0.0;
                oHatch1.PatternScale = 2.0;
                oHatch1.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                //////
                ///
                btr.AppendEntity(oHatch1);
                Tx.AddNewlyCreatedDBObject(oHatch1, true);
                Tx.AddNewlyCreatedDBObject(plBox, true);
                Tx.AddNewlyCreatedDBObject(plBox1, true);

                oHatch.Associative = true;
                oHatch.AppendLoop((int)HatchLoopTypes.Default, ObjIds);
                oHatch.EvaluateHatch(true);

                oHatch1.Associative = true;
                oHatch1.AppendLoop((int)HatchLoopTypes.Default, ObjIds1);
                oHatch1.AppendLoop((int)HatchLoopTypes.Default, ObjIds2);
                oHatch1.EvaluateHatch(true);



                Tx.Commit();
            }
        }
        public void test(double x, double y)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

            Editor ed = doc.Editor;

            Database dwg = doc.Database;

            double f = Math.Pow(x, 2) + Math.Pow(y, 2);
            if (f <= 4)
            {
                if (((x > 0 && x < 1) && (y > 0 && y < 1)) || ((x > -1 && x < 0) && (y > -1 && y < 0)))
                {
                    MessageBox.Show("Точка не входить в область");
                }
                else
                {
                    try
                    {
                        using (DocumentLock docLock = doc.LockDocument())
                        {
                            using (Transaction Tx = dwg.TransactionManager.StartTransaction())
                            {
                                ObjectId ModelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(dwg);

                                DBPoint point = new DBPoint(new Point3d(x, y, 0));

                                BlockTableRecord btr = Tx.GetObject(ModelSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                                btr.AppendEntity(point);

                                dwg.Pdmode = 3;

                                Tx.AddNewlyCreatedDBObject(point, true);

                                Tx.Commit();
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage("Problem occured because " + ex.Message.ToString());
                    }
                }
            }              
            else
            {
                MessageBox.Show("Точка не входить в область");
            }


        }

        //Lab 10 User Interface

        //Context Menu
        /*public class adskClass : Class1, IExtensionApplication
        {
            ContextMenuExtension myContextMenu;
            private void addContextMenu()
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;

                try
                {
                    myContextMenu = new ContextMenuExtension();
                    myContextMenu.Title = "Circle Jig";

                    Autodesk.AutoCAD.Windows.MenuItem mi = new Autodesk.AutoCAD.Windows.MenuItem("Run Circle Jig");

                    mi.Click += new EventHandler(CallBackOnClick);

                    myContextMenu.MenuItems.Add(mi);
                    Autodesk.AutoCAD.ApplicationServices.Application.AddDefaultContextMenuExtension(myContextMenu);
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("Error Adding Context Menu: " + ex.Message);
                }
            }
            private void RemoveContextMenu()
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                try
                {
                    if (myContextMenu != null)
                    {
                        Autodesk.AutoCAD.ApplicationServices.Application.RemoveDefaultContextMenuExtension(myContextMenu);
                        myContextMenu = null;
                    }
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("Error Remove Context Menu: " + ex.Message);
                }
            }
            private void CallBackOnClick(object sender, EventArgs e)
            {
                using (DocumentLock dockLock = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.LockDocument())
                {
                    Class1 class1 = new Class1();
                    class1.CircleJig();
                }
            }

            public void Initialize()
            {
                addContextMenu();
                AddTabDialog();
            }

            public void Terminate()
            {
                RemoveContextMenu();
            }

            //Dialog
            public static string myVariable;

            public static void AddTabDialog()
            {
                Autodesk.AutoCAD.ApplicationServices.Application.DisplayingOptionDialog += new TabbedDialogEventHandler(TabHandler);
            }
            private static void TabHandler(object sender, TabbedDialogEventArgs e)
            {
                myCustomTab myCustomTab = new myCustomTab();

                TabbedDialogAction tabbedDialogAct = new TabbedDialogAction(myCustomTab.OnOk);

                TabbedDialogExtension tabbedDialogExt = new TabbedDialogExtension(myCustomTab, tabbedDialogAct);

                e.AddTab("Value for custom variable", tabbedDialogExt);
            }

            [CommandMethod("testTab")]
            public void TestTab()
            {

                Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage(myVariable.ToString());
            }
        }*/
    }
}