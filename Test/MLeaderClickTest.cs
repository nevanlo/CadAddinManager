using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

/// <summary>
/// MLeader点击事件测试类
/// 提供基础的测试功能来验证MLeader点击事件是否正常工作
/// </summary>
public class MLeaderClickTest
{
    [CommandMethod("TESTMLEADERSETUP")]
    public static void TestMLeaderSetup()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
        {
            System.Windows.Forms.MessageBox.Show("没有活动的AutoCAD文档");
            return;
        }

        try
        {
            doc.Editor.WriteMessage("\n=== MLeader点击事件测试 ===");
            doc.Editor.WriteMessage("\n正在创建测试环境...");

            // 创建多个测试MLeader对象
            CreateTestMLeaders();

            doc.Editor.WriteMessage("\n测试环境创建完成！");
            doc.Editor.WriteMessage("\n使用以下命令进行测试:");
            doc.Editor.WriteMessage("\n  STARTMLEADERCLICK - 开始点击测试");
            doc.Editor.WriteMessage("\n  STOPMLEADERCLICK  - 停止点击测试");
            doc.Editor.WriteMessage("\n  TESTMLEADERINFO   - 显示MLeader信息");
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n测试设置失败: {ex.Message}");
        }
    }

    [CommandMethod("TESTMLEADERINFO")]
    public static void TestMLeaderInfo()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            PromptEntityOptions peo = new PromptEntityOptions("\n选择一个MLeader对象查看信息:");
            peo.SetRejectMessage("\n必须选择MLeader对象。");
            peo.AddAllowedClass(typeof(MLeader), true);

            PromptEntityResult per = doc.Editor.GetEntity(peo);

            if (per.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    MLeader mleader = tr.GetObject(per.ObjectId, OpenMode.ForRead) as MLeader;
                    if (mleader != null)
                    {
                        DisplayMLeaderInfo(mleader);
                    }
                    tr.Commit();
                }
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n获取MLeader信息失败: {ex.Message}");
        }
    }

    private static void CreateTestMLeaders()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        using (Transaction tr = doc.TransactionManager.StartTransaction())
        {
            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // 创建第一个测试MLeader
            MLeader mleader1 = CreateSampleMLeader(
                new Point3d(0, 0, 0),
                new Point3d(10, 8, 0),
                "测试MLeader 1\\P点击查看详细信息"
            );
            btr.AppendEntity(mleader1);
            tr.AddNewlyCreatedDBObject(mleader1, true);

            // 创建第二个测试MLeader
            MLeader mleader2 = CreateSampleMLeader(
                new Point3d(20, 0, 0),
                new Point3d(30, 8, 0),
                "测试MLeader 2\\P包含更多信息\\P可以编辑内容"
            );
            btr.AppendEntity(mleader2);
            tr.AddNewlyCreatedDBObject(mleader2, true);

            // 创建第三个测试MLeader
            MLeader mleader3 = CreateSampleMLeader(
                new Point3d(40, 0, 0),
                new Point3d(50, 8, 0),
                "测试MLeader 3\\P多行文本示例\\P支持换行显示\\P点击编辑"
            );
            btr.AppendEntity(mleader3);
            tr.AddNewlyCreatedDBObject(mleader3, true);

            tr.Commit();
        }
    }

    private static MLeader CreateSampleMLeader(Point3d startPoint, Point3d endPoint, string content)
    {
        MLeader mleader = new MLeader();
        mleader.SetDatabaseDefaults();
        mleader.ContentType = ContentType.MTextContent;

        // 添加引线
        int leaderIndex = mleader.AddLeader();
        mleader.AddLeaderLine(leaderIndex);
        mleader.AddFirstVertex(leaderIndex, startPoint);
        mleader.AddLastVertex(leaderIndex, endPoint);

        // 设置MText内容
        MText mtext = new MText();
        mtext.SetDatabaseDefaults();
        mtext.Contents = content;
        mtext.Location = endPoint;
        mtext.TextHeight = 2.0;
        mtext.Width = 0;

        mleader.MText = mtext;
        mleader.DoglegLength = 2.0;

        return mleader;
    }

    private static void DisplayMLeaderInfo(MLeader mleader)
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        doc.Editor.WriteMessage("\n=== MLeader 详细信息 ===");
        doc.Editor.WriteMessage($"\n对象ID: {mleader.ObjectId}");
        doc.Editor.WriteMessage($"\n内容类型: {mleader.ContentType}");
        doc.Editor.WriteMessage($"\n引线数量: {mleader.NumLeaders}");
        doc.Editor.WriteMessage($"\nDog Leg长度: {mleader.DoglegLength:F2}");

        if (mleader.ContentType == ContentType.MTextContent && mleader.MText != null)
        {
            MText mtext = mleader.MText;
            doc.Editor.WriteMessage($"\nMText信息:");
            doc.Editor.WriteMessage($"\n  文本高度: {mtext.TextHeight:F2}");
            doc.Editor.WriteMessage($"\n  文本宽度: {mtext.Width:F2}");
            doc.Editor.WriteMessage($"\n  位置: ({mtext.Location.X:F2}, {mtext.Location.Y:F2}, {mtext.Location.Z:F2})");
            doc.Editor.WriteMessage($"\n  图层: {mtext.Layer}");
            doc.Editor.WriteMessage($"\n  内容: {mtext.Contents}");
        }

        doc.Editor.WriteMessage("\n========================");
    }

    [CommandMethod("CLEARTESTMLEADERS")]
    public static void ClearTestMLeaders()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            // 选择所有MLeader对象并删除
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\n选择要删除的MLeader对象:";

            SelectionFilter filter = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "MULTILEADER")
            });

            PromptSelectionResult psr = doc.Editor.GetSelection(pso, filter);

            if (psr.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selObj in psr.Value)
                    {
                        Entity ent = tr.GetObject(selObj.ObjectId, OpenMode.ForWrite) as Entity;
                        if (ent != null)
                        {
                            ent.Erase();
                        }
                    }
                    tr.Commit();
                    doc.Editor.WriteMessage($"\n已删除 {psr.Value.Count} 个MLeader对象。");
                }
            }
            else
            {
                doc.Editor.WriteMessage("\n没有选择任何对象。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n删除MLeader对象失败: {ex.Message}");
        }
    }

    [CommandMethod("MLEADERHELP")]
    public static void ShowMLeaderHelp()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        doc.Editor.WriteMessage("\n=== MLeader点击事件插件帮助 ===");
        doc.Editor.WriteMessage("\n");
        doc.Editor.WriteMessage("\n测试命令:");
        doc.Editor.WriteMessage("\n  TESTMLEADERSETUP    - 创建测试环境");
        doc.Editor.WriteMessage("\n  TESTMLEADERINFO     - 查看MLeader信息");
        doc.Editor.WriteMessage("\n  CLEARTESTMLEADERS   - 清除测试MLeader");
        doc.Editor.WriteMessage("\n  MLEADERHELP         - 显示此帮助信息");
        doc.Editor.WriteMessage("\n");
        doc.Editor.WriteMessage("\n主要功能命令:");
        doc.Editor.WriteMessage("\n  CREATESAMPLEMLEADER - 创建示例MLeader");
        doc.Editor.WriteMessage("\n  STARTMLEADERCLICK   - 开始MLeader点击监听");
        doc.Editor.WriteMessage("\n  STOPMLEADERCLICK    - 停止MLeader点击监听");
        doc.Editor.WriteMessage("\n");
        doc.Editor.WriteMessage("\n使用步骤:");
        doc.Editor.WriteMessage("\n  1. 运行 TESTMLEADERSETUP 创建测试对象");
        doc.Editor.WriteMessage("\n  2. 运行 STARTMLEADERCLICK 开始监听");
        doc.Editor.WriteMessage("\n  3. 点击任意MLeader对象查看对话框");
        doc.Editor.WriteMessage("\n  4. 在对话框中编辑内容并保存");
        doc.Editor.WriteMessage("\n  5. 按ESC或输入X退出监听模式");
        doc.Editor.WriteMessage("\n================================");
    }
}