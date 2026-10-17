# Reefkeeper：Unity 调色与素材替换指南

适用项目：Unity 6.3 LTS / 6000.3.19f1，URP。本文是操作教程；以下建议颜色尚未应用到场景。README 保持英文。

## 先记住三个入口

- 场景：`Reefkeeper/Assets/Reefkeeper/Scenes/ShallowReef.unity`
- 专用美术面板：顶部菜单 **Reefkeeper → Art controls - 美术调节**。
- 已有材质：`Reefkeeper/Assets/Reefkeeper/Generated`。

停止 Play 后修改，旁边打开 Game 窗口观察实际相机效果。最后点击美术面板的 **保存材质与场景**。

## Unreal 与 Unity 对照

| Unreal | Unity |
| --- | --- |
| World Outliner / Details | Hierarchy / Inspector |
| Content Browser | Project |
| Actor + Components | GameObject + Components |
| Static Mesh Component | Mesh Filter 存模型，Mesh Renderer 管材质 |
| Post Process Volume | Volume + Volume Profile |

## 1. 把薄荷绿调回自然的水下颜色

当前绿色来自多层叠加：偏绿的沙地、日光、环境光和焦散，以及偏高的曝光。先恢复近处沙石的自然颜色，再用远处蓝色雾表现水体。

打开美术面板，先试这一组起点，再对照潜水照片微调。它们是美术建议值，不是已经验证的最终预设。

| 面板设置 | 建议起点 |
| --- | --- |
| 远处水色 / Fog | 灰蓝色 `#1C4B65` |
| 能见度 | 40–50 米 |
| 日光颜色 | 白色 `#FFFFFF` |
| 日光强度 | 1.2；原始值为 2.1 |
| 曝光 | -0.2；原始值为 +0.35 |
| 饱和度 | -15；原始值为 +8 |

点击颜色块，在颜色选择器的 Hex 栏输入色号。曝光控制明暗，饱和度控制颜色浓度；只降饱和度不能消除所有绿色光照。

### 沙地和焦散

在面板点击 **沙地 Sand**：

- **Tint**：先试偏灰沙色 `#BDB7A4`。
- **Caustic Color**：地上亮水纹的颜色。先接近白色，HDR 的 Intensity 设为 0；仍然抢眼就继续降低亮度。这里的 0 是 HDR 曝光档位，不代表关掉焦散。
- **Main Texture**：可放沙地颜色贴图。有贴图后先将 Tint 设为白色，避免再次把贴图染绿。

点击 **岩石 Rock** 可用同样方式调整。近处沙地与岩石保留本色，远处逐渐融入水色；焦散只提供适量的明暗变化。

### 环境光

专用面板尚未包含环境光。打开 **Window → Rendering → Lighting → Environment → Environment Lighting**，当前为 Gradient：

| 设置 | 建议起点 |
| --- | --- |
| Sky Color | 灰蓝 `#899CA6` |
| Equator Color | 深灰蓝 `#4E626F` |
| Ground Color | 深灰 `#2C3539` |

它们主要影响环境照明和阴影里的颜色。每次只调整一组，观察 Game 窗口。

### 原生后处理入口

Hierarchy 中选 **Underwater color grade**，Inspector 的 Volume Profile 内含 Color Adjustments、ACES Tonemapping、Bloom 和 Vignette。专用面板的曝光与饱和度就在修改这个 Profile。

Unity 官方说明：[Color Adjustments](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/Post-Processing-Color-Adjustments.html)。

## 2. 先用一块岩石练习替换模型

1. 在 Project 窗口新建自己的素材文件夹，将 FBX 和贴图一起拖入。Blender 模型建议先导出 FBX。
2. Hierarchy 中选中一块 **Reef limestone**，按 F 聚焦。
3. 将新模型拖到旧岩石下面，作为子物体。
4. 调整新模型的本地 Position、Rotation 和 Scale，与旧模型对齐。旧物体可能已有缩放，新模型会继承它，需要一起检查。
5. 只关闭旧岩石的 **Mesh Renderer**，先保留父物体、原有碰撞和地形跟随引用。
6. 若轮廓差异明显，再调整碰撞体。不要让新旧碰撞体不必要地重叠。

对于仅一个 Mesh 的简单物体，也可直接替换 Mesh Filter 的 Mesh，但原来的 MeshCollider 不会自动匹配新模型，仍需同步修改。

新增独立地面物体后，如果希望它随地形调节升降，应将其根物体登记到海床 **ReefSeabed → Grounded Props**。不要同时登记父物体和子物体。

Unity 官方说明：[模型导入](https://docs.unity3d.com/6000.0/Documentation/Manual/ImportingModelFiles.html)。

## 3. 替换材质和贴图

选新模型中真正带 **Mesh Renderer** 的那层，将材质拖入 **Materials → Element 0**。有多个材质槽时逐个对应。

### 保留现有动态焦散

在 Generated 中复制 `Limestone.mat` 或 `Sand.mat`，改副本的 **Main Texture、Tint、Caustic Color**，再赋给新模型。当前 WaterUnder 材质的公开贴图接口较简单，完整 PBR 贴图接入可能需要扩展 Shader Graph。

### 使用素材自带的 PBR 质感

优先选素材自带的 URP 材质；也可新建 Material，将 Shader 设为 **Universal Render Pipeline/Lit**：

| 贴图/属性 | URP/Lit 入口 |
| --- | --- |
| Albedo / Base Color | Base Map |
| Normal | Normal Map；贴图导入类型设为 Normal map |
| AO | Occlusion Map |
| 表面粗糙程度 | Smoothness；数值方向与 Roughness 相反 |

粗糙岩石的 Smoothness 可先试 0.15–0.3。独立 Roughness 贴图不能直接当作默认 Metallic 贴图使用；通常需要反相并按目标材质约定打包到 Smoothness 通道。

普通 URP/Lit 材质会接收场景灯光和雾，但不会自动得到现有材质中的动态焦散。若出现亮粉色，先检查是否用了 Built-in 或 HDRP 专用 Shader。

Unity 官方说明：[URP/Lit](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/lit-shader.html)。

## 4. 换刷子和架子时保留交互

### 刷子

在 **Restoration brush** 下放新外观模型，只隐藏旧外观的 Renderer。保留：

- Rigidbody、XRGrabInteractable、ReefBrush 和所需碰撞体。
- **Grip pose**：手握位置。
- **Bristle contact point**：刷毛接触点。

让新模型刷毛对准接触点；若修改握姿或刷毛位置，要一起调整对应 Transform，并试抓取和刷洗。

### 架子

架子的 **Algae patch** 是清洁判定区域，带触发器和 RackDirtPatch。新横梁的位置、方向、宽度改变后，需要同步调整这些区域。仅换外观且尺寸相近时，保留原判定最省事。

## 保存与回退

- 使用专用面板的 **保存材质与场景**；Ctrl+S 保存当前场景。
- 共享材质改一次会影响所有使用者；只想改单个物体就先复制材质。
- 复制场景不会自动复制共享材质或 Volume Profile。想保留完全独立的美术版本，需要一起复制它们，或使用 Git 里程碑回退。
- Play 中场景改动通常在退出时还原；材质等资源修改可能保留，因此统一在停止 Play 后做美术调整。
- **不要点击 Create or rebuild shallow reef**：这是完整场景生成器，会覆盖手工布局和生成资源。
- 初始交互版本：[milestone-04-xr-workshop](https://github.com/CaitlynJiang/MOLTEN_hackathon/tree/milestone-04-xr-workshop)。照片参考后的美术版本：[milestone-06-blue-nursery](https://github.com/CaitlynJiang/MOLTEN_hackathon/tree/milestone-06-blue-nursery)。
