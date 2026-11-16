# 旋锋涤尘：核心玩法代码

[▶ 在线观看实机演示](https://haonan-123-zc.github.io/unity-2d-survival-gameplay-demo/)（[下载视频](demo/实机演示.mp4)）

本项目是从《旋锋涤尘》源码整理的 Unity C# 核心逻辑代码包。
提供一套可阅读的代码详解

## 技术栈

- Unity 6000.3.11f1
- C#
- Unity 2D Physics
- UGUI 与 TextMeshPro
- Unity 2D Animation
- Unity Video Player

## 目录

```text
Assets/_Project/Scripts
├── Audio        # 跨场景音频服务、BGM 淡入淡出、SFX 限流
├── Combat       # 碰撞伤害、弹丸、受击反馈、屏幕震动
├── Enemies      # 近战敌人、远程鬼魂、刷怪器、Boss
├── Flow         # 菜单、视频、教程、死亡和胜利流程
├── Player       # 移动、生命、成长、技能、武器、进化
├── Progression  # 经验、拾取物、进化配置、传送门
├── UI           # 材质流光效果
└── World        # 无限地图分块、交互场景树
```

## 架构概览

### 1. 玩家状态链路

```text
经验宝石 -> PlayerStats.GainExp
             ├─ 升级 -> OrbitWeaponController
             └─ 每 5 级 -> PlayerEvolutionManager
                           ├─ PlayerEvolutionFX
                           ├─ 角色模型和武器外观
                           └─ MapManager 地图主题

伤害来源 -> PlayerHealth.TakeDamage
              ├─ PlayerParrySkill 无敌窗口
              ├─ HitFlash / HealthShakeTrigger
              └─ DeathSceneDirector
```

`PlayerHealth` 是生命值规则的唯一入口。伤害使用 `TakeDamage`，恢复使用 `Heal`，
不再通过传入负数伤害间接治疗，避免规则分散到拾取物和 UI 代码中。

### 2. 敌人与关卡压力

- `EnemyController`：近战敌人追击、受击、死亡和掉落。
- `GhostController`：远程鬼魂采用“追击 / 保持距离”的状态逻辑。
- `EnemySpawner`：按等级提升生成频率、批量数量和生命倍率，并从相机外侧投放敌人。
- `MechaGorillaBoss`：使用协程组织跳跃预警、砸地、击退、电波和死亡演出。
- `FinalWaveDirector`：用多个显式状态标志控制“停止普通刷怪 -> 生成终波 -> 清场 -> 胜利”。

### 3. 暂停与演出

游戏暂停或进入结算时，`Time.deltaTime` 会变为零。
需要继续推进的 UI、镜头和过场动画统一使用 `Time.unscaledDeltaTime`，
避免死亡、胜利和进化演出在 `Time.timeScale = 0` 时卡住。

### 4. 地图分块

`MapManager` 只维护玩家周围 5x5 个地图块：

1. 将玩家世界坐标换算成格点坐标。
2. 仅在跨格时更新地图。
3. 用 `HashSet` 计算需要保留的格点。
4. 实例化缺失地块并销毁离屏地块。

相比每帧全量创建或线性搜索，这种方式将常规更新成本降到只在跨格时发生。

## 导入方式

1. 使用 Unity 6000.3.11f1 创建 2D 或 URP 2D 工程。
2. 将本目录中的 `Assets` 合并到新工程根目录。
3. 在 Package Manager 安装或启用：

```text
com.unity.ugui
com.unity.textmeshpro
com.unity.2d.animation
com.unity.modules.video
```

4. 在 Input Manager 中配置 `Horizontal`、`Vertical` 输入轴。
5. 创建 `Player` Tag，并按代码中的 `[Tooltip]` 和 `[Header]` 配置组件引用。
6. 新建测试场景，从最简单的“玩家移动 + 一个敌人”开始逐项接入。

## 重要说明

这些代码由原 Demo 整理，并非原始工程文件。

