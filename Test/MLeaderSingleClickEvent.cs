using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

public class MLeaderSingleClickEvent
{
    private static bool _eventHandlerAttached = false;

    [CommandMethod("STARTMLEADERCLICK")]
    public static void StartMLeaderClickEvent()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            if (!_eventHandlerAttached)
            {
                // 启动选择循环
                _eventHandlerAttached = true;
                doc.Editor.WriteMessage("\nMLeader单击监听已启动。点击MLeader的MText区域显示对话框，按ESC退出。");
                StartSelectionLoop();
            }
            else
            {
                doc.Editor.WriteMessage("\nMLeader单击监听已经在运行中。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
            _eventHandlerAttached = false;
        }
    }

    [CommandMethod("STOPMLEADERCLICK")]
    public static void StopMLeaderClickEvent()
    {
        _eventHandlerAttached = false;
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc != null)
        {
            doc.Editor.WriteMessage("\nMLeader单击监听已停止。");
        }
    }

    [CommandMethod("CREATESAMPLEMLEADER")]
    public static void CreateSampleMLeader()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // 创建MLeader对象
                MLeader mleader = new MLeader();
                mleader.SetDatabaseDefaults();

                // 设置MLeader属性
                mleader.ContentType = ContentType.MTextContent;
                
                // 添加引线点
                Point3d startPoint = new Point3d(0, 0, 0);
                Point3d endPoint = new Point3d(8, 5, 0);
                
                int leaderIndex = mleader.AddLeader();
                mleader.AddLeaderLine(leaderIndex);
                mleader.AddFirstVertex(leaderIndex, startPoint);
                mleader.AddLastVertex(leaderIndex, endPoint);

                // 设置MText内容
                MText mtext = new MText();
                mtext.SetDatabaseDefaults();
                mtext.Contents = "单击这里！\\P这是一个可单击的\\PMLeader文本示例";
                mtext.Location = endPoint;
                mtext.TextHeight = 2.5;
                mtext.Width = 0;

                // 将MText设置为MLeader的内容
                mleader.MText = mtext;
                mleader.DoglegLength = 2.0;

                // 添加到数据库
                BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                
                btr.AppendEntity(mleader);
                tr.AddNewlyCreatedDBObject(mleader, true);
                
                tr.Commit();
                doc.Editor.WriteMessage("\n示例MLeader已创建。使用STARTMLEADERCLICK命令开始监听点击事件。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
        }
    }

    private static void StartSelectionLoop()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        // 在后台线程中运行选择循环
        System.Threading.Tasks.Task.Run(() =>
        {
            while (_eventHandlerAttached)
            {
                try
                {
                    // 使用Invoke确保在UI线程中执行
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.Invoke(() =>
                    {
                        if (!_eventHandlerAttached)
                            return;

                        PromptEntityOptions peo = new PromptEntityOptions("\n点击MLeader对象 [退出(X)]:");
                        peo.SetRejectMessage("\n必须选择MLeader对象。");
                        peo.AddAllowedClass(typeof(MLeader), true);
                        peo.AllowNone = false;
                        peo.Keywords.Add("X", "X", "退出(X)");

                        PromptEntityResult per = doc.Editor.GetEntity(peo);

                        if (per.Status == PromptStatus.OK)
                        {
                            HandleMLeaderSelection(per.ObjectId);
                        }
                        else if (per.Status == PromptStatus.Keyword && per.StringResult == "X")
                        {
                            _eventHandlerAttached = false;
                            doc.Editor.WriteMessage("\nMLeader单击监听已退出。");
                        }
                        else if (per.Status == PromptStatus.Cancel)
                        {
                            _eventHandlerAttached = false;
                            doc.Editor.WriteMessage("\nMLeader单击监听已取消。");
                        }
                    });

                    // 短暂延迟避免过度占用CPU
                    System.Threading.Thread.Sleep(100);
                }
                catch (System.Exception ex)
                {
                    Autodesk.AutoCAD.ApplicationServices.Core.Application.Invoke(() =>
                    {
                        doc.Editor.WriteMessage($"\n选择循环错误: {ex.Message}");
                    });
                    _eventHandlerAttached = false;
                }
            }
        });
    }

    private static void HandleMLeaderSelection(ObjectId mleaderId)
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                MLeader mleader = tr.GetObject(mleaderId, OpenMode.ForRead) as MLeader;
                
                if (mleader != null && mleader.ContentType == ContentType.MTextContent)
                {
                    // 显示对话框
                    ShowMLeaderDialog(mleader);
                }
                else
                {
                    doc.Editor.WriteMessage("\n选中的MLeader没有MText内容。");
                }
                
                tr.Commit();
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage($"\n处理MLeader选择时出错: {ex.Message}");
        }
    }

    private static void ShowMLeaderDialog(MLeader mleader)
    {
        try
        {
            // 创建并显示对话框
            SimpleMLeaderDialog dialog = new SimpleMLeaderDialog(mleader);
            
            DialogResult result = dialog.ShowDialog();
            
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                if (result == DialogResult.OK)
                {
                    doc.Editor.WriteMessage($"\n对话框已关闭，用户输入: {dialog.UserInput}");
                }
                else
                {
                    doc.Editor.WriteMessage("\n对话框已取消。");
                }
            }
        }
        catch (System.Exception ex)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage($"\n显示对话框时出错: {ex.Message}");
            }
        }
    }
}

// 简化的对话框类
public class SimpleMLeaderDialog : Form
{
    private MLeader _mleader;
    private TextBox _textBox;
    private Button _okButton;
    private Button _cancelButton;
    private Label _infoLabel;
    private RichTextBox _infoTextBox;

    public string UserInput { get; private set; } = string.Empty;

    public SimpleMLeaderDialog(MLeader mleader)
    {
        _mleader = mleader;
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        // 设置窗体属性
        this.Text = "MLeader 信息和编辑对话框";
        this.Size = new System.Drawing.Size(450, 320);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // 标题标签
        _infoLabel = new Label()
        {
            Text = "MLeader 详细信息:",
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(400, 20),
            Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
        };

        // 信息显示区域
        _infoTextBox = new RichTextBox()
        {
            Location = new System.Drawing.Point(20, 45),
            Size = new System.Drawing.Size(400, 120),
            ReadOnly = true,
            Text = GetMLeaderDetailedInfo(),
            Font = new System.Drawing.Font("Consolas", 8F),
            BackColor = System.Drawing.SystemColors.Control
        };

        // 输入标签
        Label inputLabel = new Label()
        {
            Text = "编辑MText内容:",
            Location = new System.Drawing.Point(20, 180),
            Size = new System.Drawing.Size(150, 20),
            Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
        };

        // 文本输入框
        _textBox = new TextBox()
        {
            Location = new System.Drawing.Point(20, 205),
            Size = new System.Drawing.Size(400, 40),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = GetCurrentMTextContent()
        };

        // 按钮
        _okButton = new Button()
        {
            Text = "更新并关闭",
            Location = new System.Drawing.Point(245, 255),
            Size = new System.Drawing.Size(90, 30),
            DialogResult = DialogResult.OK,
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8F)
        };

        _cancelButton = new Button()
        {
            Text = "取消",
            Location = new System.Drawing.Point(345, 255),
            Size = new System.Drawing.Size(75, 30),
            DialogResult = DialogResult.Cancel,
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8F)
        };

        // 添加事件处理器
        _okButton.Click += OkButton_Click;
        _cancelButton.Click += CancelButton_Click;

        // 添加控件到窗体
        this.Controls.AddRange(new Control[] { 
            _infoLabel, _infoTextBox, inputLabel, _textBox, _okButton, _cancelButton 
        });

        // 设置默认按钮
        this.AcceptButton = _okButton;
        this.CancelButton = _cancelButton;
    }

    private string GetMLeaderDetailedInfo()
    {
        try
        {
            string info = $"对象类型: {_mleader.GetType().Name}\n";
            info += $"内容类型: {_mleader.ContentType}\n";
            info += $"引线数量: {_mleader.NumLeaders}\n";
            info += $"Dog Leg长度: {_mleader.DoglegLength:F2}\n";
            
            if (_mleader.ContentType == ContentType.MTextContent && _mleader.MText != null)
            {
                MText mtext = _mleader.MText;
                info += $"文本高度: {mtext.TextHeight:F2}\n";
                info += $"文本宽度: {mtext.Width:F2}\n";
                info += $"位置: ({mtext.Location.X:F2}, {mtext.Location.Y:F2}, {mtext.Location.Z:F2})\n";
                info += $"旋转角度: {mtext.Rotation * 180 / Math.PI:F1}°\n";
                info += $"图层: {mtext.Layer}\n";
                info += $"颜色索引: {mtext.ColorIndex}";
            }
            
            return info;
        }
        catch (System.Exception ex)
        {
            return $"获取MLeader信息时出错: {ex.Message}";
        }
    }

    private string GetCurrentMTextContent()
    {
        try
        {
            if (_mleader.ContentType == ContentType.MTextContent && _mleader.MText != null)
            {
                // 转换AutoCAD的换行符到Windows换行符
                return _mleader.MText.Contents.Replace("\\P", Environment.NewLine);
            }
            return string.Empty;
        }
        catch (System.Exception ex)
        {
            return $"读取内容出错: {ex.Message}";
        }
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        UserInput = _textBox.Text;
        
        // 更新MLeader的MText内容
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
                        // 转换Windows换行符到AutoCAD换行符
                        mtext.Contents = _textBox.Text.Replace(Environment.NewLine, "\\P");
                        mleader.MText = mtext;
                        
                        // 强制更新显示
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
            MessageBox.Show($"更新MLeader内容时出错:\n{ex.Message}", "更新错误", 
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