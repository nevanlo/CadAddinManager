using System;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Test
{
    public class MLeaderClickHandler
    {
        // 命令：启动MLeader点击监听器
        [CommandMethod("STARTMLEADERCLICK")]
        public void StartMLeaderClickListener()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            
            ed.WriteMessage("\n开始监听MLeader点击事件...");
            ed.WriteMessage("\n输入ESC或STOPMLEADERCLICK命令停止监听");
            
            // 注册点击事件处理器
            doc.ImpliedSelectionChanged += OnImpliedSelectionChanged;
            
            ed.WriteMessage("\nMLeader点击监听器已启动！");
        }
        
        // 命令：停止MLeader点击监听器
        [CommandMethod("STOPMLEADERCLICK")]
        public void StopMLeaderClickListener()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            
            // 取消注册点击事件处理器
            doc.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
            
            ed.WriteMessage("\nMLeader点击监听器已停止！");
        }
        
        // 选择变化事件处理器
        private void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
            Document doc = sender as Document;
            if (doc == null) return;
            
            Editor ed = doc.Editor;
            
            try
            {
                // 获取当前选择集
                PromptSelectionResult selResult = ed.SelectImplied();
                if (selResult.Status != PromptStatus.OK || selResult.Value.Count == 0)
                    return;
                
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject selObj in selResult.Value)
                    {
                        if (selObj != null)
                        {
                            Entity entity = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                            
                            // 检查是否为MLeader对象
                            if (entity is MLeader mleader)
                            {
                                HandleMLeaderClick(mleader, doc);
                                break; // 只处理第一个MLeader
                            }
                        }
                    }
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n处理MLeader点击事件时出错: {ex.Message}");
            }
        }
        
        // 处理MLeader点击事件
        private void HandleMLeaderClick(MLeader mleader, Document doc)
        {
            try
            {
                // 获取MLeader的MText内容
                string mtextContent = GetMLeaderText(mleader);
                
                // 显示对话框
                ShowMLeaderDialog(mtextContent, mleader.ObjectId, doc);
            }
            catch (System.Exception ex)
            {
                doc.Editor.WriteMessage($"\n处理MLeader时出错: {ex.Message}");
            }
        }
        
        // 获取MLeader的文本内容
        private string GetMLeaderText(MLeader mleader)
        {
            try
            {
                if (mleader.ContentType == ContentType.MTextContent)
                {
                    MText mtext = mleader.MText;
                    if (mtext != null)
                    {
                        return mtext.Contents;
                    }
                }
                return "无文本内容";
            }
            catch
            {
                return "读取文本失败";
            }
        }
        
        // 显示MLeader信息对话框
        private void ShowMLeaderDialog(string textContent, ObjectId mleaderId, Document doc)
        {
            // 创建并显示对话框
            MLeaderInfoForm dialog = new MLeaderInfoForm(textContent, mleaderId, doc);
            
            // 使用AutoCAD主窗口作为父窗口
            IntPtr acadHandle = Application.MainWindow.Handle;
            
            if (acadHandle != IntPtr.Zero)
            {
                dialog.StartPosition = FormStartPosition.CenterParent;
                NativeWindow nativeWindow = new NativeWindow();
                nativeWindow.AssignHandle(acadHandle);
                dialog.ShowDialog(nativeWindow);
                nativeWindow.ReleaseHandle();
            }
            else
            {
                dialog.ShowDialog();
            }
        }
        
        // 命令：手动选择MLeader并显示信息
        [CommandMethod("SELECTMLEADER")]
        public void SelectMLeaderManually()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            
            try
            {
                // 提示用户选择MLeader
                PromptEntityOptions peo = new PromptEntityOptions("\n请选择一个MLeader对象: ");
                peo.SetRejectMessage("\n所选对象不是MLeader！");
                peo.AddAllowedClass(typeof(MLeader), true);
                
                PromptEntityResult per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                    return;
                
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    MLeader mleader = trans.GetObject(per.ObjectId, OpenMode.ForRead) as MLeader;
                    if (mleader != null)
                    {
                        HandleMLeaderClick(mleader, doc);
                    }
                    trans.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\n选择MLeader时出错: {ex.Message}");
            }
        }
    }
    
    // MLeader信息对话框
    public partial class MLeaderInfoForm : Form
    {
        private ObjectId _mleaderId;
        private Document _document;
        
        public MLeaderInfoForm(string textContent, ObjectId mleaderId, Document doc)
        {
            _mleaderId = mleaderId;
            _document = doc;
            
            InitializeComponent();
            LoadMLeaderInfo(textContent);
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "MLeader 信息";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // 创建控件
            Label lblText = new Label();
            lblText.Text = "MText 内容:";
            lblText.Location = new System.Drawing.Point(12, 15);
            lblText.Size = new System.Drawing.Size(80, 20);
            this.Controls.Add(lblText);
            
            TextBox txtContent = new TextBox();
            txtContent.Name = "txtContent";
            txtContent.Multiline = true;
            txtContent.ScrollBars = ScrollBars.Vertical;
            txtContent.Location = new System.Drawing.Point(12, 40);
            txtContent.Size = new System.Drawing.Size(460, 100);
            txtContent.ReadOnly = true;
            this.Controls.Add(txtContent);
            
            Label lblInfo = new Label();
            lblInfo.Text = "MLeader 属性:";
            lblInfo.Location = new System.Drawing.Point(12, 150);
            lblInfo.Size = new System.Drawing.Size(100, 20);
            this.Controls.Add(lblInfo);
            
            TextBox txtInfo = new TextBox();
            txtInfo.Name = "txtInfo";
            txtInfo.Multiline = true;
            txtInfo.ScrollBars = ScrollBars.Vertical;
            txtInfo.Location = new System.Drawing.Point(12, 175);
            txtInfo.Size = new System.Drawing.Size(460, 150);
            txtInfo.ReadOnly = true;
            this.Controls.Add(txtInfo);
            
            Button btnEdit = new Button();
            btnEdit.Text = "编辑文本";
            btnEdit.Location = new System.Drawing.Point(12, 335);
            btnEdit.Size = new System.Drawing.Size(80, 25);
            btnEdit.Click += BtnEdit_Click;
            this.Controls.Add(btnEdit);
            
            Button btnClose = new Button();
            btnClose.Text = "关闭";
            btnClose.Location = new System.Drawing.Point(392, 335);
            btnClose.Size = new System.Drawing.Size(80, 25);
            btnClose.DialogResult = DialogResult.OK;
            this.Controls.Add(btnClose);
            
            this.ResumeLayout(false);
        }
        
        private void LoadMLeaderInfo(string textContent)
        {
            try
            {
                TextBox txtContent = this.Controls["txtContent"] as TextBox;
                TextBox txtInfo = this.Controls["txtInfo"] as TextBox;
                
                if (txtContent != null)
                {
                    txtContent.Text = textContent;
                }
                
                if (txtInfo != null)
                {
                    using (Transaction trans = _document.TransactionManager.StartTransaction())
                    {
                        MLeader mleader = trans.GetObject(_mleaderId, OpenMode.ForRead) as MLeader;
                        if (mleader != null)
                        {
                            string info = GetMLeaderProperties(mleader);
                            txtInfo.Text = info;
                        }
                        trans.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"加载MLeader信息时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private string GetMLeaderProperties(MLeader mleader)
        {
            try
            {
                string info = "";
                info += $"对象ID: {mleader.ObjectId}\n";
                info += $"图层: {mleader.Layer}\n";
                info += $"颜色: {mleader.Color}\n";
                info += $"线型: {mleader.Linetype}\n";
                info += $"内容类型: {mleader.ContentType}\n";
                info += $"引线数量: {mleader.NumLeaderLines}\n";
                
                if (mleader.ContentType == ContentType.MTextContent)
                {
                    info += $"文本高度: {mleader.TextHeight}\n";
                    info += $"文本样式: {mleader.TextStyleId}\n";
                }
                
                // 获取引线点
                for (int i = 0; i < mleader.NumLeaderLines; i++)
                {
                    info += $"引线 {i + 1} 顶点数: {mleader.NumVerticesInLeaderLine(i)}\n";
                }
                
                return info;
            }
            catch (System.Exception ex)
            {
                return $"获取属性时出错: {ex.Message}";
            }
        }
        
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox txtContent = this.Controls["txtContent"] as TextBox;
                if (txtContent == null) return;
                
                string currentText = txtContent.Text;
                
                // 创建简单的文本编辑对话框
                using (TextEditForm editForm = new TextEditForm(currentText))
                {
                    if (editForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // 更新MLeader的文本
                        UpdateMLeaderText(editForm.EditedText);
                        txtContent.Text = editForm.EditedText;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"编辑文本时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateMLeaderText(string newText)
        {
            try
            {
                using (Transaction trans = _document.TransactionManager.StartTransaction())
                {
                    MLeader mleader = trans.GetObject(_mleaderId, OpenMode.ForWrite) as MLeader;
                    if (mleader != null && mleader.ContentType == ContentType.MTextContent)
                    {
                        MText mtext = mleader.MText;
                        if (mtext != null)
                        {
                            mtext.Contents = newText;
                        }
                    }
                    trans.Commit();
                }
                
                // 刷新AutoCAD视图
                _document.Editor.Regen();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"更新MLeader文本失败: {ex.Message}");
            }
        }
    }
    
    // 文本编辑对话框
    public partial class TextEditForm : Form
    {
        public string EditedText { get; private set; }
        
        public TextEditForm(string initialText)
        {
            InitializeComponent();
            
            TextBox txtEdit = this.Controls["txtEdit"] as TextBox;
            if (txtEdit != null)
            {
                txtEdit.Text = initialText;
                EditedText = initialText;
            }
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // 窗体设置
            this.Text = "编辑文本";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // 文本框
            TextBox txtEdit = new TextBox();
            txtEdit.Name = "txtEdit";
            txtEdit.Multiline = true;
            txtEdit.ScrollBars = ScrollBars.Vertical;
            txtEdit.Location = new System.Drawing.Point(12, 12);
            txtEdit.Size = new System.Drawing.Size(360, 200);
            this.Controls.Add(txtEdit);
            
            // 确定按钮
            Button btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.Location = new System.Drawing.Point(217, 225);
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Click += (s, e) => {
                TextBox txt = this.Controls["txtEdit"] as TextBox;
                if (txt != null) EditedText = txt.Text;
            };
            this.Controls.Add(btnOK);
            
            // 取消按钮
            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Location = new System.Drawing.Point(297, 225);
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
            
            this.ResumeLayout(false);
        }
    }
}