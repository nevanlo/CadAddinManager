# AutoCAD MLeader 点击事件插件

这个插件实现了对AutoCAD中MLeader对象的MText区域的鼠标点击响应功能。当用户点击MLeader的文本区域时，会弹出一个对话框显示MLeader的详细信息，并允许用户编辑MText内容。

## 功能特性

1. **单击响应**: 点击MLeader的MText区域弹出信息对话框
2. **双击响应**: 双击MLeader的MText区域弹出信息对话框（备选方案）
3. **信息显示**: 显示MLeader的详细属性信息
4. **内容编辑**: 允许用户编辑和更新MText内容
5. **实时更新**: 修改后的内容会立即在AutoCAD中更新显示

## 文件说明

- `MLeaderClickEvent.cs`: 双击事件实现版本
- `MLeaderSingleClickEvent.cs`: 单击事件实现版本（推荐）

## 可用命令

### 单击事件版本（推荐）

1. **CREATESAMPLEMLEADER** - 创建示例MLeader
   - 在图纸中创建一个带有示例文本的MLeader对象
   - 用于测试点击事件功能

2. **STARTMLEADERCLICK** - 启动MLeader点击监听
   - 开始监听MLeader对象的点击事件
   - 点击任何MLeader对象都会弹出信息对话框
   - 输入"X"或按ESC键退出监听模式

3. **STOPMLEADERCLICK** - 停止MLeader点击监听
   - 停止监听MLeader对象的点击事件

### 双击事件版本

1. **CREATEMLEADER** - 创建示例MLeader
2. **ATTACHMLEADERCLICK** - 附加双击事件处理器
3. **DETACHMLEADERCLICK** - 分离双击事件处理器

## 使用步骤

### 方法一：使用现有MLeader对象

1. 确保图纸中有MLeader对象（带有MText内容）
2. 在AutoCAD命令行输入：`STARTMLEADERCLICK`
3. 点击任意MLeader对象
4. 在弹出的对话框中查看信息或编辑文本内容
5. 点击"更新并关闭"保存修改，或点击"取消"放弃修改
6. 完成后输入"X"或按ESC键退出监听模式

### 方法二：使用示例MLeader对象

1. 在AutoCAD命令行输入：`CREATESAMPLEMLEADER`
2. 系统会自动创建一个示例MLeader对象
3. 输入：`STARTMLEADERCLICK`
4. 点击刚创建的MLeader对象
5. 在对话框中编辑内容并保存

## 对话框功能

对话框包含以下信息和功能：

### 信息显示区域
- 对象类型
- 内容类型
- 引线数量
- Dog Leg长度
- 文本高度和宽度
- 文本位置坐标
- 旋转角度
- 图层信息
- 颜色索引

### 编辑功能
- 多行文本编辑框
- 支持换行符处理
- 实时更新到AutoCAD图纸
- 错误处理和用户提示

## 技术实现

### 事件处理机制
- 使用AutoCAD .NET API的Editor事件
- 支持PointMonitor和BeginDoubleClick事件
- 异步处理避免界面阻塞

### 对话框设计
- Windows Forms界面
- 响应式布局
- 用户友好的操作体验

### 数据处理
- Transaction管理确保数据一致性
- 错误处理和异常捕获
- 换行符格式转换（AutoCAD \\P <-> Windows Environment.NewLine）

## 系统要求

- AutoCAD 2020或更高版本
- .NET Framework 4.8 或 .NET 8.0（取决于AutoCAD版本）
- Windows操作系统

## 安装方法

1. 编译项目生成DLL文件
2. 在AutoCAD中使用NETLOAD命令加载DLL
3. 或者将DLL文件放置在AutoCAD的插件目录中自动加载

## 注意事项

1. **线程安全**: 插件使用了适当的线程同步机制
2. **内存管理**: 正确使用了Transaction和Dispose模式
3. **错误处理**: 包含完整的异常处理机制
4. **用户体验**: 提供了清晰的操作提示和反馈

## 扩展可能

1. **右键菜单**: 可以扩展为右键菜单选项
2. **批量编辑**: 支持同时编辑多个MLeader对象
3. **模板功能**: 预定义常用的MText内容模板
4. **样式设置**: 允许用户自定义对话框样式和行为
5. **导入导出**: 支持MLeader内容的导入导出功能

## 故障排除

### 常见问题

1. **对话框不显示**
   - 检查是否正确启动了点击监听
   - 确认点击的是MLeader对象而不是其他图元

2. **内容更新失败**
   - 检查MLeader对象是否被锁定
   - 确认当前用户有修改权限

3. **程序崩溃**
   - 检查AutoCAD版本兼容性
   - 查看错误日志获取详细信息

### 调试模式

在开发模式下，可以通过以下方式获取更多调试信息：
- 查看AutoCAD命令行的错误消息
- 使用Visual Studio调试器附加到AutoCAD进程
- 启用.NET异常详细信息

## 版权说明

此插件基于AutoCAD .NET API开发，仅供学习和参考使用。