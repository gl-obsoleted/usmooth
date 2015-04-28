
# Release Notes

## v0.2a [2015-04-28] [下载页面](/releases/tag/0.2a)

- (new feature) 可使用复选框单独控制任意一个 Mesh 是否渲染
- (internal) 使用增量更新来降低监控数据的传输量，为后面实时刷新做性能准备

## v0.1.2 [2015-04-17] 

- (new feature) 在 usmooth 内浅蓝色高亮显示当前编辑器内选中的物件
- (new feature) 当高亮显示选中物件时，同时浅蓝色高亮显示出跟该物件关联的对应的材质和贴图 
- (realtime page) 移除 Mesh 栏的模型名（实践中基本上跟物件名重复）
- (ui) 把 Realtime 页面改为每个列表单独一个滚动条；简化 UI 层级关系，减少上方空白区域
- (net) strip 掉超长的名字，packet 支持浮点数写入
- (net) 在包头附上 packet 长度用于分包和拼包的实现

## v0.1.1 [2015-04-15] 

- (new feature) 实现定位物体功能 - “选中物件时在编辑器的 Scene View 内飞到该物件并 lookat ”
- (new feature) 实现 Texture -> Material -> Mesh 反向关系查询和浅绿色高亮 
- (new feature) 实现 Mesh -> Material -> Texture 正向关系查询和浅绿色高亮
- (internal) 移除对 ucore 的依赖，简化结构
- (internal) 简化和分拆 Realtime 页面内的代码，降低复杂度

## v0.1 [2015-04-13] 

- (new feature) 可获取游戏运行时当前屏幕内可见的 Mesh / Material / Texture 三级信息
- (net) 基本的 Protocol/Packet 实现
- (net) 收发/断线/保活/最近连接列表等等逻辑实现








