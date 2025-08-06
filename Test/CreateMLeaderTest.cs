using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test
{
    public class CreateMLeaderTest
    {
        [CommandMethod("CREATEMLEADERTEST")]
        public void CreateMLeaderForTest()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                // 提示用户点击引线起点
                PromptPointOptions ppo1 = new PromptPointOptions("\n请指定引线起点: ");
                PromptPointResult ppr1 = ed.GetPoint(ppo1);
                if (ppr1.Status != PromptStatus.OK) return;

                // 提示用户点击引线终点
                PromptPointOptions ppo2 = new PromptPointOptions("\n请指定引线终点: ");
                ppo2.BasePoint = ppr1.Value;
                ppo2.UseBasePoint = true;
                PromptPointResult ppr2 = ed.GetPoint(ppo2);
                if (ppr2.Status != PromptStatus.OK) return;

                // 提示用户输入文本内容
                PromptStringOptions pso = new PromptStringOptions("\n请输入文本内容: ");
                pso.AllowSpaces = true;
                pso.DefaultValue = "这是一个测试MLeader";
                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK) return;

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // 创建MLeader对象
                    MLeader mleader = new MLeader();
                    
                    // 设置MLeader属性
                    mleader.SetDatabaseDefaults();
                    mleader.ContentType = ContentType.MTextContent;
                    
                    // 添加引线
                    int leaderIndex = mleader.AddLeaderLine(ppr1.Value);
                    mleader.AddFirstVertex(leaderIndex, ppr1.Value);
                    mleader.AddLastVertex(leaderIndex, ppr2.Value);
                    
                    // 创建MText内容
                    MText mtext = new MText();
                    mtext.SetDatabaseDefaults();
                    mtext.Contents = pr.StringResult;
                    mtext.TextHeight = 3.5;
                    mtext.Location = ppr2.Value;
                    
                    // 设置MLeader的MText
                    mleader.MText = mtext;
                    
                    // 添加到数据库
                    btr.AppendEntity(mleader);
                    trans.AddNewlyCreatedDBObject(mleader, true);
                    
                    trans.Commit();
                    
                    ed.WriteMessage($"\n已成功创建测试MLeader，ObjectId: {mleader.ObjectId}");
                    ed.WriteMessage("\n现在可以使用STARTMLEADERCLICK命令启动点击监听器进行测试");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n创建MLeader时出错: {ex.Message}");
            }
        }

        [CommandMethod("CREATEMULTIPLEMLEADERS")]
        public void CreateMultipleMLeadersForTest()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    // 创建多个测试MLeader
                    string[] testTexts = {
                        "测试MLeader 1\n这是第一个测试对象",
                        "测试MLeader 2\n这是第二个测试对象\n包含多行文本",
                        "测试MLeader 3\n简单文本",
                        "测试MLeader 4\n用于测试点击事件\n和对话框功能"
                    };

                    Point3d basePoint = new Point3d(0, 0, 0);
                    double spacing = 20.0;

                    for (int i = 0; i < testTexts.Length; i++)
                    {
                        // 计算位置
                        Point3d startPoint = new Point3d(basePoint.X, basePoint.Y + i * spacing, 0);
                        Point3d endPoint = new Point3d(basePoint.X + 15, basePoint.Y + i * spacing + 5, 0);

                        // 创建MLeader
                        MLeader mleader = new MLeader();
                        mleader.SetDatabaseDefaults();
                        mleader.ContentType = ContentType.MTextContent;

                        // 添加引线
                        int leaderIndex = mleader.AddLeaderLine(startPoint);
                        mleader.AddFirstVertex(leaderIndex, startPoint);
                        mleader.AddLastVertex(leaderIndex, endPoint);

                        // 创建MText内容
                        MText mtext = new MText();
                        mtext.SetDatabaseDefaults();
                        mtext.Contents = testTexts[i];
                        mtext.TextHeight = 3.5;
                        mtext.Location = endPoint;

                        // 设置MLeader的MText
                        mleader.MText = mtext;

                        // 添加到数据库
                        btr.AppendEntity(mleader);
                        trans.AddNewlyCreatedDBObject(mleader, true);
                    }

                    trans.Commit();

                    ed.WriteMessage($"\n已成功创建 {testTexts.Length} 个测试MLeader对象");
                    ed.WriteMessage("\n使用ZOOM EXTENTS查看所有对象");
                    ed.WriteMessage("\n使用STARTMLEADERCLICK命令启动点击监听器进行测试");
                    
                    // 自动缩放到全部对象
                    ed.Command("ZOOM", "EXTENTS");
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n创建多个MLeader时出错: {ex.Message}");
            }
        }
    }
}