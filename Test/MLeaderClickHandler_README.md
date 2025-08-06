# MLeader 点击事件处理器使用说明

## 功能概述

这个AutoCAD插件实现了MLeader（多重引线）对象的鼠标单击事件处理功能。当用户点击MLeader对象时，会弹出一个对话框显示MLeader的详细信息，包括MText内容和各种属性。

## 主要功能

1. **自动监听MLeader点击事件**
2. **显示MLeader信息对话框**
3. **编辑MLeader文本内容**
4. **手动选择MLeader查看信息**

## 可用命令

### 1. STARTMLEADERCLICK
启动MLeader点击监听器
- 功能：开始监听图纸中MLeader对象的点击事件
- 使用：在AutoCAD命令行输入 `STARTMLEADERCLICK`
- 效果：启动后，点击任何MLeader对象都会弹出信息对话框

### 2. STOPMLEADERCLICK
停止MLeader点击监听器
- 功能：停止监听MLeader点击事件
- 使用：在AutoCAD命令行输入 `STOPMLEADERCLICK`
- 效果：停止自动监听功能

### 3. SELECTMLEADER
手动选择MLeader查看信息
- 功能：手动选择一个MLeader对象并显示其信息
- 使用：在AutoCAD命令行输入 `SELECTMLEADER`
- 效果：提示用户选择MLeader，然后显示信息对话框

## 测试命令

### 4. CREATEMLEADERTEST
创建单个测试MLeader
- 功能：交互式创建一个MLeader对象用于测试
- 使用：在AutoCAD命令行输入 `CREATEMLEADERTEST`
- 效果：提示用户指定引线起点、终点和文本内容

### 5. CREATEMULTIPLEMLEADERS
创建多个测试MLeader
- 功能：自动创建4个不同的MLeader对象用于测试
- 使用：在AutoCAD命令行输入 `CREATEMULTIPLEMLEADERS`
- 效果：在图纸中创建4个测试MLeader并自动缩放视图

## 对话框功能

### MLeader信息对话框
- **MText内容显示**：显示MLeader中的文本内容
- **属性信息显示**：显示MLeader的各种属性（图层、颜色、线型等）
- **编辑文本功能**：可以直接编辑MLeader的文本内容
- **实时更新**：编辑后的内容会立即更新到AutoCAD图纸中

### 文本编辑对话框
- **多行文本编辑**：支持多行文本的编辑
- **确定/取消**：可以保存或取消编辑

## 使用步骤

1. **编译插件**
   ```
   在Visual Studio中编译Test项目
   ```

2. **加载插件**
   ```
   在AutoCAD中使用NETLOAD命令加载编译后的DLL文件
   ```

3. **创建测试对象**（可选）
   ```
   输入命令：CREATEMULTIPLEMLEADERS
   这会创建4个测试MLeader对象
   ```

4. **启动监听**
   ```
   输入命令：STARTMLEADERCLICK
   ```

5. **点击MLeader**
   ```
   在图纸中点击任何MLeader对象，会自动弹出信息对话框
   ```

6. **查看和编辑**
   ```
   在对话框中查看MLeader信息，可以点击"编辑文本"按钮修改内容
   ```

7. **停止监听**（可选）
   ```
   输入命令：STOPMLEADERCLICK
   ```

## 技术特点

1. **事件驱动**：使用AutoCAD的ImpliedSelectionChanged事件
2. **安全处理**：包含完整的异常处理机制
3. **用户友好**：对话框设计简洁直观
4. **实时更新**：编辑后立即更新到图纸
5. **多版本支持**：支持AutoCAD 2020-2025版本

## 注意事项

1. 插件需要在AutoCAD环境中运行
2. 确保图纸中有MLeader对象才能测试功能
3. 编辑文本功能只对包含MText内容的MLeader有效
4. 建议在使用前保存图纸，以防意外修改

## 扩展可能

这个基础框架可以扩展为：
- 批量编辑MLeader文本
- MLeader样式管理
- 自定义MLeader属性编辑
- MLeader统计和报告功能