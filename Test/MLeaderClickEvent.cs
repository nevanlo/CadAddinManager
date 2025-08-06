using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

public class MLeaderClickEvent
{
    private static bool _eventHandlerAttached = false;

    [CommandMethod("ATTACHMLEADERCLICK")]
    public static void AttachMLeaderClickEvent()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            if (!_eventHandlerAttached)
            {
                // 附加点击事件处理器
                doc.Editor.PointMonitor += Editor_PointMonitor;
                doc.Editor.BeginDoubleClick += Editor_BeginDoubleClick;
                _eventHandlerAttached = true;
                doc.Editor.WriteMessage("\nMLeader点击事件已附加。双击MLeader的MText区域将显示对话框。");
            }
            else
            {
                doc.Editor.WriteMessage("\nMLeader点击事件已经附加。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
        }
    }

    [CommandMethod("DETACHMLEADERCLICK")]
    public static void DetachMLeaderClickEvent()
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            if (_eventHandlerAttached)
            {
                // 分离点击事件处理器
                doc.Editor.PointMonitor -= Editor_PointMonitor;
                doc.Editor.BeginDoubleClick -= Editor_BeginDoubleClick;
                _eventHandlerAttached = false;
                doc.Editor.WriteMessage("\nMLeader点击事件已分离。");
            }
            else
            {
                doc.Editor.WriteMessage("\nMLeader点击事件未附加。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
        }
    }

    [CommandMethod("CREATEMLEADER")]
    public static void CreateMLeader()
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
                Point3d endPoint = new Point3d(5, 5, 0);
                
                int leaderIndex = mleader.AddLeader();
                mleader.AddLeaderLine(leaderIndex);
                mleader.AddFirstVertex(leaderIndex, startPoint);
                mleader.AddLastVertex(leaderIndex, endPoint);

                // 设置MText内容
                MText mtext = new MText();
                mtext.SetDatabaseDefaults();
                mtext.Contents = "点击我！\\P这是一个可点击的MLeader文本";
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
                doc.Editor.WriteMessage("\nMLeader已创建。使用ATTACHMLEADERCLICK命令附加点击事件。");
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError: " + ex.ToString());
        }
    }

    private static void Editor_BeginDoubleClick(object sender, BeginDoubleClickEventArgs e)
    {
        Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
            return;

        try
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                // 获取双击位置的实体
                PromptSelectionResult selResult = doc.Editor.SelectAtPoint(e.Location);
                
                if (selResult.Status == PromptStatus.OK && selResult.Value.Count > 0)
                {
                    foreach (SelectedObject selObj in selResult.Value)
                    {
                        Entity ent = tr.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                        
                        if (ent is MLeader mleader)
                        {
                            // 检查是否点击在MText区域
                            if (IsPointInMTextArea(mleader, e.Location))
                            {
                                // 显示对话框
                                ShowMLeaderDialog(mleader);
                                e.Handled = true;
                                break;
                            }
                        }
                    }
                }
                
                tr.Commit();
            }
        }
        catch (System.Exception ex)
        {
            doc.Editor.WriteMessage("\nError in double click handler: " + ex.ToString());
        }
    }

    private static void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
    {
        // 这个方法可以用于在鼠标移动时提供视觉反馈
        // 当前实现为空，可以根据需要添加功能
    }

    private static bool IsPointInMTextArea(MLeader mleader, Point3d clickPoint)
    {
        try
        {
            if (mleader.ContentType != ContentType.MTextContent)
                return false;

            // 获取MText的边界
            MText mtext = mleader.MText;
            if (mtext == null)
                return false;

            // 简单的边界检查 - 检查点击点是否在MText位置附近
            Point3d mtextLocation = mtext.Location;
            double tolerance = mtext.TextHeight * 2; // 容差范围

            double distance = clickPoint.DistanceTo(mtextLocation);
            return distance <= tolerance;
        }
        catch
        {
            return false;
        }
    }

    private static void ShowMLeaderDialog(MLeader mleader)
    {
        try
        {
            // 创建并显示对话框
            MLeaderDialog dialog = new MLeaderDialog(mleader);
            
            // 使用AutoCAD的主窗口作为父窗口
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(dialog);
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // 如果用户点击了OK，可以在这里处理结果
                Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage($"\n对话框已关闭，返回值: {dialog.UserInput}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                doc.Editor.WriteMessage("\nError showing dialog: " + ex.ToString());
            }
        }
    }
}

// 自定义对话框类
public partial class MLeaderDialog : Form
{
    private MLeader _mleader;
    private TextBox _textBox;
    private Button _okButton;
    private Button _cancelButton;
    private Label _infoLabel;

    public string UserInput { get; private set; } = string.Empty;

    public MLeaderDialog(MLeader mleader)
    {
        _mleader = mleader;
        InitializeDialog();
    }

    private void InitializeDialog()
    {
        // 设置窗体属性
        this.Text = "MLeader 属性对话框";
        this.Size = new System.Drawing.Size(400, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // 创建控件
        _infoLabel = new Label()
        {
            Text = "MLeader信息:",
            Location = new System.Drawing.Point(20, 20),
            Size = new System.Drawing.Size(350, 20),
            Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold)
        };

        Label contentLabel = new Label()
        {
            Text = GetMLeaderInfo(),
            Location = new System.Drawing.Point(20, 45),
            Size = new System.Drawing.Size(350, 60),
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8F)
        };

        Label inputLabel = new Label()
        {
            Text = "输入新的文本内容:",
            Location = new System.Drawing.Point(20, 115),
            Size = new System.Drawing.Size(150, 20)
        };

        _textBox = new TextBox()
        {
            Location = new System.Drawing.Point(20, 140),
            Size = new System.Drawing.Size(350, 20),
            Text = GetCurrentMTextContent()
        };

        _okButton = new Button()
        {
            Text = "确定",
            Location = new System.Drawing.Point(215, 180),
            Size = new System.Drawing.Size(75, 25),
            DialogResult = DialogResult.OK
        };

        _cancelButton = new Button()
        {
            Text = "取消",
            Location = new System.Drawing.Point(295, 180),
            Size = new System.Drawing.Size(75, 25),
            DialogResult = DialogResult.Cancel
        };

        // 添加事件处理器
        _okButton.Click += OkButton_Click;
        _cancelButton.Click += CancelButton_Click;

        // 添加控件到窗体
        this.Controls.AddRange(new Control[] { 
            _infoLabel, contentLabel, inputLabel, _textBox, _okButton, _cancelButton 
        });

        // 设置默认按钮
        this.AcceptButton = _okButton;
        this.CancelButton = _cancelButton;
    }

    private string GetMLeaderInfo()
    {
        try
        {
            string info = $"类型: {_mleader.ContentType}\n";
            info += $"引线数量: {_mleader.NumLeaders}\n";
            
            if (_mleader.ContentType == ContentType.MTextContent && _mleader.MText != null)
            {
                info += $"文本高度: {_mleader.MText.TextHeight:F2}\n";
                info += $"位置: ({_mleader.MText.Location.X:F2}, {_mleader.MText.Location.Y:F2})";
            }
            
            return info;
        }
        catch
        {
            return "无法获取MLeader信息";
        }
    }

    private string GetCurrentMTextContent()
    {
        try
        {
            if (_mleader.ContentType == ContentType.MTextContent && _mleader.MText != null)
            {
                return _mleader.MText.Contents.Replace("\\P", "\n"); // 转换换行符
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        UserInput = _textBox.Text;
        
        // 可以在这里更新MLeader的MText内容
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
                        mtext.Contents = _textBox.Text.Replace("\n", "\\P"); // 转换换行符
                        mleader.MText = mtext;
                    }
                    tr.Commit();
                }
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"更新MLeader内容时出错: {ex.Message}", "错误", 
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