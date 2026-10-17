# Reefkeeper 免费海底素材候选清单

核查日期：2026-10-17（Pacific）。共 27 个素材、素材包或模型库入口。点击名称查看原作者的图片、3D 预览或演示。

这是选型清单：已查看来源页面或其检索记录，尚未下载、导入或在 Unity 6000.3.19f1 中测试。免费与授权以下载时页面及包内许可为准。无法完整核实的条目已标出。

## 时间少，先看这五个

1. **[Poly Haven — Coral Ground 02](https://polyhaven.com/a/coral_ground_02)**：CC0 海床贴图，优先替换现在大面积的纯色沙地。
2. **[Poly Haven — Coast Rocks 01](https://polyhaven.com/a/coast_rocks_01)**：CC0 写实海岸岩石模型；原始版本很重，挑 LOD 或减面后用。
3. **[Smithsonian — Coral Shape & Structure](https://3d.si.edu/corals/shape-structure)**：真实珊瑚形态模型库，适合挑种植用珊瑚苗和观察结构；多为标本，需要重新处理活体颜色与面数。
4. **[Quaternius — LowPoly Animated Fish](https://quaternius.itch.io/lowpoly-animated-fish)**：CC0、已经绑骨和带游泳动画，今天最快加入游动生物的路线。
5. **[VivianMenard — Boids](https://github.com/VivianMenard/boids)**：MIT、URP 鱼群工程，包含避障、捕食者交互、程序动画和水下视觉。原版是 Unity 2022.3.16f1，需要做 Unity 6 适配检查。

我的建议组合是：**写实地表贴图 + 少数重点扫描珊瑚 + 简单鱼模型和 Boids 行为**。先搭一个局部好看的工作区，不必一次替换整片海床。

## A. 写实礁石 / 石壁模型

| # | 素材与预览 | 页面标注 | 适合怎么用 / 接入成本 |
| --- | --- | --- | --- |
| 01 | [Coast Rocks 01 — Poly Haven](https://polyhaven.com/a/coast_rocks_01) | 免费，CC0；模型，带 PBR 贴图；约 100 万三角面，页面列出 LODs | 海床大岩群与背景轮廓。先用低 LOD，不要把最高精度复制几十份。页面提供 Blender、glTF、FBX 等下载选项。 |
| 02 | [Beach cliff rock face — matousekfoto](https://sketchfab.com/3d-models/beach-cliff-rock-face-cceaa2c8a2f04e91953765e47790f02a) | 免费下载，CC Attribution；249k 三角面，8K 贴图，摄影测量 | 石灰岩壁、礁石边缘；减面并缩小贴图后更适合 laptop。 |
| 03 | [Sea cliff rock face — axonite](https://sketchfab.com/3d-models/sea-cliff-rock-face-e4bf57abe35f4df3b6f4c0f32507091f) | 免费下载，CC Attribution；538.5k 三角面，海岸石灰岩扫描 | 用作少量背景大块岩面；需要减面。 |

这些来源的石头是海岸岩石，并不是所有模型都来自水下珊瑚礁。使用时可调整苔藓/陆地痕迹，并用我们的水下光照统一。

## B. 写实海床 / 礁石 PBR 贴图

下面都是**贴图，不是可以直接拖进场景的 3D 珊瑚模型**。Poly Haven 的资产采用 [CC0](https://polyhaven.com/license)。

| # | 素材与预览 | 用途建议 |
| --- | --- | --- |
| 04 | [Coral Ground 02](https://polyhaven.com/a/coral_ground_02) | 珊瑚碎屑、粗糙海床；优先看。 |
| 05 | [Coral Gravel](https://polyhaven.com/a/coral_gravel) | 小块珊瑚碎石，工作区边缘和礁石脚下。 |
| 06 | [Coral Mud 01](https://polyhaven.com/a/coral_mud_01) | 较暗、较脏的沉积地表，减少整片地面过于干净的感觉。 |
| 07 | [Aerial Beach 01](https://polyhaven.com/a/aerial_beach_01) | 沙地大面积底层；色调需结合参考照片。 |
| 08 | [Coast Sand Rocks 02](https://polyhaven.com/a/coast_sand_rocks_02) | 沙与石的过渡区域。 |
| 09 | [Rock Boulder Dry](https://polyhaven.com/a/rock_boulder_dry) | 给现有程序岩石补表面细节；是干岩纹理，要重新调水下观感。 |
| 10 | [Rock Face 03](https://polyhaven.com/a/rock_face_03) | 石壁或大块礁石表面。 |

下载建议：先选 1K 或 2K。Base Color、Normal、Roughness/AO 足够开始；不要默认全部下载 8K。现有 WaterUnder 材质可直接换 Main Texture，但完整法线、粗糙度等 PBR 通道还需扩展图或使用 URP/Lit；普通 URP/Lit 不会自动获得我们当前的焦散。

## C. 真实珊瑚 / 扫描与科学模型

| # | 素材与预览 | 免费与授权情况 | 用途 / 成本 |
| --- | --- | --- | --- |
| 11 | [Smithsonian — Shape & Structure](https://3d.si.edu/corals/shape-structure) | Smithsonian Open Access 珊瑚项目；进入具体模型确认 CC0 标记和下载规格 | 大量板状、枝状、块状等形态。适合找真实结构和取苗模型；标本外观不等于活体珊瑚颜色。 |
| 12 | [The Hydrous — Open Access Coral Models](https://sketchfab.com/thehydro.us/collections/open-access-coral-models-available-for-download-2f969e2bc6664d3698135418b5f2d318) | 检索记录列出 9 个可下载模型；合集详情抓取失败，单模型授权仍需确认 | 包括 Acropora、Pocillopora、Montipora 等；先打开 3D 预览挑形态。不要把整个合集默认当成同一许可。 |
| 13 | [Brain Coral — Platygyra daedalea / jordanmakesmaps](https://sketchfab.com/3d-models/brain-coral-platygyra-daedalea-0e9aa3ac05034dba8ebd3d0b8f24357a) | 免费下载，CC Attribution；246.8k 三角面 | 菲律宾水下拍摄重建的脑珊瑚，适合少量近景重点物；需要减面与贴图检查。 |
| 14 | [Coral scan — stuff scanner](https://sketchfab.com/3d-models/coral-269d537956d44a0085b720cd8b6ff2de) | 免费下载，CC Attribution；669.5k 三角面 | 海滩上一大块珊瑚的扫描；可用于枯珊瑚/骨骼形态，面数高。 |
| 15 | [Acropora sp. skeleton — TheRLab](https://sketchfab.com/3d-models/acropora-sp-skeleton-94d23931902c414bbf698d09d90e1efb) | 免费下载，CC Attribution；759.7k 三角面 | UCD 标本摄影测量，适合真实枝状结构；做游戏需要减面和材质处理。 |

Smithsonian 项目的下载说明见其 [Digitization Program Office 报告](https://dpo.si.edu/sites/default/files/resources/OCIO%20DPO%20Annual%20Report%202020-PRINT_version.pdf)：从各标本的 Voyager Link / View Model 进入下载。也可浏览 [珊瑚项目总页](https://3d.si.edu/corals?order=title&sort=desc)。

## D. 可以快速拼场景的模型 / 整包

| # | 素材与预览 | 内容 | 免费与授权 / 注意事项 |
| --- | --- | --- | --- |
| 16 | [Lowpoly coral — assetfactory](https://sketchfab.com/3d-models/lowpoly-coral-864594b70b6740efa479f259de547fbc) | 单个珊瑚，8.9k 三角面 | 检索页面显示免费、Free Standard；不是 CC0，下载前看其具体许可。详情抓取失败，未验证材质或格式。 |
| 17 | [Coral Reef — libblekibble](https://sketchfab.com/3d-models/coral-reef-9492bf6c1ab7471f99a87a32d21f12fe) | 完整 stylized 海底小场景，约 172k 三角面 | 页面记录为免费、CC Attribution，并有 NoAI 标记；适合手工导入和参考，不作为生成式 AI 输入。是否可方便拆分须看下载文件。 |
| 18 | [Underwater Assets 12-pack — Wadud](https://wadud.itch.io/underwaterassets) | 12 个网格、4K 贴图；有 Unity 贴图包。Mesh 包 5 MB，Unity 贴图包约 747 MB | Name your own price，免费候选；**公开页面未写明完整使用许可**，选中后需查包内 LICENSE/作者说明。未标 URP 支持，不能当作即插即用。 |
| 19 | [Ocean Assets — Starter Pack / Atomic Realm](https://atomicrealm.itch.io/ocean-assets-starter) | 10+ 低模海洋物件：植物、小珊瑚、海葵、船与潜艇等；FBX/OBJ/glTF，单张 1K 图集 | 可免费获取；允许商业项目和修改，需署名，不允许单独再分发素材。部分额外物件只在作者 Discord 提供，基础下载不是全目录。 |
| 20 | [Aquatic Animal Models — Riley](https://rkuhlf-assets.itch.io/aquatic-animal-models) | 绑骨/动画动物；11 种岩石、7 种珊瑚、4 种贝壳、4 种鱼、3 种海草；FBX/Blend/Unity 包 | 可免费获取；允许商业项目和修改，署名可选，禁止转售/再分发素材。低模风格，演示场景不含；Unity 包的 URP 兼容未验证。 |

## E. 鱼模型与动画

| # | 素材与预览 | 内容 | 免费与授权 / 接入成本 |
| --- | --- | --- | --- |
| 21 | [LowPoly Animated Fish — Quaternius](https://quaternius.itch.io/lowpoly-animated-fish) | 绑骨海洋动物，每个模型有游泳动画；FBX/OBJ/Blend，约 1 MB | CC0。最快的动画素材选项；偏 stylized。**游泳动画不等于自动鱼群 AI**。 |
| 22 | [PS1/PSX Fish Pack — CursedLake](https://cursedlake.itch.io/ps1psx-fish-pack) | 8 种鱼，FBX/GLB，约 2 MB；含小丑鱼、梭鱼、金枪鱼，也混有淡水鱼 | 免费，可商业使用和修改，署名可选，禁止素材再分发；**无骨骼/动画**。作者披露 AI 辅助纹理。适合远景小鱼，不能直接充当逼真热带鱼全集。 |

Riley 的整包也含鱼和大型动物，见 #20。鱼模型决定外观，动画负责摆尾，AI 决定往哪里游；这三部分需要接在一起。

## F. 鱼群 AI / Boids

这里的 AI 指自动群游、避障等游戏行为，无需接在线大模型。

| # | 项目与演示入口 | 已有功能 | 许可与适配判断 |
| --- | --- | --- | --- |
| 23 | [VivianMenard/boids](https://github.com/VivianMenard/boids) | 群游、障碍物避让、捕食者交互、程序动画，以及鱼/鲨鱼模型、水、焦散、光束；README 有视频和可玩演示 | MIT；作者明确 Unity 2022.3.16f1 + URP。**最值得先试**，但需要选择性移植，Unity 6 编译及实际性能尚未测试。 |
| 24 | [NVJOB Simple Boids](https://github.com/nvjob/nvjob-boids) | 群游、危险反应、鱼 Shader 动画；含 3 种鱼模型、5 个例子 | MIT；作者标注 Unity 2019.1.8 起。原 Shader 在我们的 URP 下需检查/改写，行为脚本可作为参考。作者称为 pseudo Boids。 |
| 25 | [Sebastian Lague — Boids](https://github.com/SebLague/Boids) | Unity Boids 实验与视频教程，适合研究规则并换成自己的鱼模型 | MIT；教学/实验代码，不是成套海底美术包。作者也指出官方 ECS 示例是更高性能的参考；今天先不额外引入 ECS。 |

演示：[NVJOB 鱼群视频](https://www.youtube.com/watch?v=NmCknqw_zHo)、[Sebastian Lague Boids 视频](https://www.youtube.com/watch?v=bqtqltqcQhw)。Vivian 的视频入口在仓库 README。

建议先展示 20–40 条小鱼并测帧率，再决定增加数量。这个数是本项目的起始试验建议，不是来源提供的性能保证。

## G. 可选的海底叙事物件

| # | 素材与预览 | 内容与许可 | 用途 |
| --- | --- | --- | --- |
| 26 | [Sunk Boat — Just-Peeps](https://sketchfab.com/3d-models/sunk-boat-1a7ea3a4abf647b5858fb3f9b9cb0121) | 免费下载，CC Attribution；约 2k 三角面，作者描述含 PBR 材质 | 可放远处作探索目标，先核实实际贴图与视觉质量。 |
| 27 | [Treasure Chest — Just-Peeps](https://sketchfab.com/3d-models/treasure-chest-b65f06769a774d9c9ecb3bc9bec46371) | 免费下载，CC Attribution；约 6.7k 三角面，作者描述有锈蚀/海生附着质感 | 备用探索道具；珊瑚修复 MVP 不必加入。 |

## 下载与接入小抄

- itch.io 的 Name your own price 页面通常可通过 **No thanks, just take me to the downloads** 免费下载；Quaternius 这一页已核实有该选项。
- Sketchfab 的下载可能需要登录。优先 FBX；若只有 GLB/glTF，可经 Blender 导出 FBX，或后续配置对应 Unity 导入器。
- 素材写着支持 Unity，不等于支持 Unity 6 + URP；优先检查材质 Shader，亮粉色通常需要修材质。
- 用普通 PBR 材质后，我们当前的材质内焦散不会自动跟过去；需要后续统一接焦散。
- 真实扫描件先减面、做简单 Collider；不要直接给鱼群使用几十万面模型。
- 外部完整 Unity 项目先单独打开，选择性移植，不覆盖 Reefkeeper 的 Packages、ProjectSettings 或主场景。
- CC Attribution 条目保存作者、原链接和许可证信息，便于写 credits。禁止再分发的素材不要直接提交到公开 GitHub；文档链接可正常保存。
- 此清单只收集候选链接，没有导入任何新模型，也没有改变你的场景色调。

## 本轮不列为免费推荐

- **Yughues Underwater Plants v.2**：当前商店检索结果显示付费，不能因为作者另有 Free Bushes 就当它免费。
- **Kenney Fish Pack**：免费 CC0，但属于 2D 素材，不是 VR 场景用的 3D 鱼。
- **UnderWater 3D / Gigachu**：当前付费。页面“free to use in commercial games”描述的是买后的使用许可，并不是免费获取。
