using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

/// <summary>
/// 简化的MLeader点击演示
/// 这是一个更简单直接的实现，便于理解和使用
/// </summary>
public class SimpleMLeaderDemo
{
    [CommandMethod("DEMO")]
    public static void RunDemo()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        doc.Editor.WriteMessage("\n=== MLeader点击演示 ===");
        doc.Editor.WriteMessage("\n1. 创建示例MLeader");
        doc.Editor.WriteMessage("\n2. 点击MLeader弹出对话框");
        doc.Editor.WriteMessage("\n3. 编辑内容并保存");
        
        // 创建示例MLeader
        CreateDemoMLeader();
        
        // 开始点击监听
        StartClickDemo();
    }

    private static void CreateDemoMLeader()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // 创建MLeader
                MLeader mleader = new MLeader();
                mleader.SetDatabaseDefaults();
                mleader.ContentType = ContentType.MTextContent;

                // 设置引线点
                Point3d startPoint = new Point3d(5, 5, 0);
                Point3d endPoint = new Point3d(15, 10, 0);
                
                int leaderIndex = mleader.AddLeader();
                mleader.AddLeaderLine(leaderIndex);
                mleader.AddFirstVertex(leaderIndex, startPoint);
                mleader.AddLastVertex(leaderIndex, endPoint);

                // 创建MText
                MText mtext = new MText();
                mtext.SetDatabaseDefaults();
                mtext.Contents = "演示MLeader\\P点击我打开对话框\\P可以编辑这些文字";
                mtext.Location = endPoint;
                mtext.TextHeight = 2.5;
                mtext.Width = 0;

                mleader.MText = mtext;
                mleader.DoglegLength = 2.0;

                // 添加到数据库
                BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                
                btr.AppendEntity(mleader);
                tr.AddNewlyCreatedDBObject(mleader, true);
                
                tr.Commit();
                doc.Editor.WriteMessage("\n演示MLeader已创建！");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n创建MLeader失败: {ex.Message}");
        }
    }

    private static void StartClickDemo()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        doc.Editor.WriteMessage("\n现在点击MLeader对象...");

        try
        {
            PromptEntityOptions peo = new PromptEntityOptions("\n点击MLeader对象打开编辑对话框:");
            peo.SetRejectMessage("\n请选择MLeader对象。");
            peo.AddAllowedClass(typeof(MLeader), true);
            peo.AllowNone = false;

            PromptEntityResult per = doc.Editor.GetEntity(peo);

            if (per.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    MLeader mleader = tr.GetObject(per.ObjectId, OpenMode.ForRead) as MLeader;
                    
                    if (mleader != null && mleader.ContentType == ContentType.MTextContent)
                    {
                        // 显示编辑对话框
                        ShowEditDialog(mleader);
                    }
                    else
                    {
                        doc.Editor.WriteMessage("\n选中的MLeader没有文本内容。");
                    }
                    
                    tr.Commit();
                }
            }
            else if (per.Status == PromptStatus.Cancel)
            {
                doc.Editor.WriteMessage("\n演示已取消。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n演示过程出错: {ex.Message}");
        }
    }

    private static void ShowEditDialog(MLeader mleader)
    {
        try
        {
            // 创建简单的编辑对话框
            DemoEditDialog dialog = new DemoEditDialog(mleader);
            DialogResult result = dialog.ShowDialog();
            
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                if (result == DialogResult.OK)
                {
                    doc.Editor.WriteMessage("\n内容已更新！");
                    doc.Editor.WriteMessage("\n演示完成。再次运行DEMO命令重新开始。");
                }
                else
                {
                    doc.Editor.WriteMessage("\n编辑已取消。");
                }
            }
        }
        catch (System.Exception ex)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage($"\n显示对话框出错: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// 演示用的简单编辑对话框
/// </summary>
public class DemoEditDialog : Form
{
    private MLeader _mleader;
    private TextBox _contentTextBox;
    private Button _updateButton;
    private Button _cancelButton;
    private Label _titleLabel;

    public DemoEditDialog(MLeader mleader)
    {
        _mleader = mleader;
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        // 窗体设置
        this.Text = "MLeader 内容编辑器 - 演示版";
        this.Size = new System.Drawing.Size(400, 200);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // 标题
        _titleLabel = new Label()
        {
            Text = "编辑MLeader文本内容:",
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(350, 20),
            Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
        };

        // 文本编辑框
        _contentTextBox = new TextBox()
        {
            Location = new System.Drawing.Point(20, 50),
            Size = new System.Drawing.Size(350, 60),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = GetCurrentContent()
        };

        // 更新按钮
        _updateButton = new Button()
        {
            Text = "更新内容",
            Location = new System.Drawing.Point(220, 125),
            Size = new System.Drawing.Size(75, 30),
            DialogResult = DialogResult.OK
        };

        // 取消按钮
        _cancelButton = new Button()
        {
            Text = "取消",
            Location = new System.Drawing.Point(305, 125),
            Size = new System.Drawing.Size(65, 30),
            DialogResult = DialogResult.Cancel
        };

        // 事件处理
        _updateButton.Click += UpdateButton_Click;
        _cancelButton.Click += CancelButton_Click;

        // 添加控件
        this.Controls.AddRange(new Control[] { 
            _titleLabel, _contentTextBox, _updateButton, _cancelButton 
        });

        // 设置默认按钮
        this.AcceptButton = _updateButton;
        this.CancelButton = _cancelButton;
    }

    private string GetCurrentContent()
    {
        try
        {
            if (_mleader.ContentType == ContentType.MTextContent && _mleader.MText != null)
            {
                return _mleader.MText.Contents.Replace("\\P", Environment.NewLine);
            }
            return "无内容";
        }
        catch
        {
            return "读取内容失败";
        }
    }

    private void UpdateButton_Click(object sender, EventArgs e)
    {
        try
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    MLeader mleader = tr.GetObject(_mleader.ObjectId, OpenMode.ForWrite) as MLeader;
                    if (mleader != null && mleader.MText != null)
                    {
                        MText mtext = mleader.MText;
                        mtext.Contents = _contentTextBox.Text.Replace(Environment.NewLine, "\\P");
                        mleader.MText = mtext;
                        
                        // 强制刷新显示
                        doc.TransactionManager.QueueForGraphicsFlush();
                    }
                    tr.Commit();
                }
                
                MessageBox.Show("MLeader内容已成功更新！", "更新成功", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"更新失败:\n{ex.Message}", "错误", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}