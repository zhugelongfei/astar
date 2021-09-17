# A Star for unity(C#)

# 介绍
A*寻路算法，及弗洛伊德路线平滑算法。
此工程为Unity Package的Git包，可通过Unity的PackageManagerWindow导入到需要使用的项目中。

# Unity导入步骤
- 在Unity中点击菜单栏的Window->Package Manager打开PackageManager面板
- 点击Add package from git url
- 复制git工程的地址，粘贴到输入栏
- 点击Add

# 使用说明
- 请查阅内置示例

# 依赖说明
- 此工程依赖于ObjectPool插件，请先导入ObjectPool
- ObjectPool插件地址为：git@github.com:zhugelongfei/objectpool.git#1.0.0

# 佛洛依德平滑算法规则（如不使用，可忽略此处）
- 地图左上角为0，0点
- 坐标系X轴的正方向为右，Y轴的正方向为下
- 坐标点不支持负数