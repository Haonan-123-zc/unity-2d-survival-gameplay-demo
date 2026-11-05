# 面试技术说明

## 可讲解的技术点

### 1. 物理帧与渲染帧分离

代码位置：`Player/PlayerController.cs`

- `Update` 只采样输入与更新 Animator。
- `FixedUpdate` 使用 `Rigidbody2D.MovePosition`。
- 输入归一化后，斜向移动不会比直线移动更快。

面试表达：把输入采样与物理更新分离，避免帧率波动改变角色移动距离，同时保留刚体碰撞检测。

### 2. 复杂攻击流程的状态约束

代码位置：`Enemies/MechaGorillaBoss.cs`

- `isAttacking` 防止攻击协程重入。
- 跳跃砸地期间关闭本体碰撞，落地前恢复。
- 红圈预警与真正伤害半径使用同一套配置。
- 死亡流程由 `isDead` 保证只执行一次。

面试表达：没有为了一个 Boss 引入过重框架，而是用显式状态锁和协程保证多阶段攻击的原子性。

### 3. 暂停状态下的演出系统

代码位置：`Flow/DeathSceneDirector.cs`、`Flow/FinalWaveDirector.cs`、`Player/PlayerEvolutionFX.cs`

- 游戏暂停后使用 `WaitForSecondsRealtime` 和 `unscaledDeltaTime`。
- UI 动画、镜头缩放和文字闪烁不会被 `timeScale = 0` 冻结。

面试表达：区分游戏时间与真实时间的职责，避免结算流程依赖已暂停的时间轴。

### 4. 低成本的程序化地图

代码位置：`World/MapManager.cs`

- 5x5 地图块随玩家格点位置动态加载。
- `HashSet` 用于计算目标地块集合。
- 只在跨格时刷新，而不是每帧创建地图对象。

面试表达：以有限的工作集维持无限地图视觉，在实现复杂度和运行时开销之间取平衡。

### 5. 数据驱动的成长和进化

代码位置：`Player/PlayerStats.cs`、`Player/PlayerEvolutionManager.cs`、`Progression/EvolutionStage.cs`

- 升级曲线与阶段触发点独立于 UI。
- 进化阶段使用可序列化列表配置角色、武器与地图主题。
- 进化表现通过回调在角色被白屏遮挡时切换，避免外观切换被玩家看到。

面试表达：规则与表现通过公开方法和回调衔接，后续可以把阶段配置迁移为 ScriptableObject。

### 6. 战斗反馈与音效控制

代码位置：`Audio/AudioManager.cs`、`Combat/HealthShakeTrigger.cs`

- BGM 先淡出再换 Clip。
- 命中音效按时间窗口限流，避免同一帧大量受击导致音量叠加。
- 屏幕震动使用真实时间结束并恢复投影矩阵和全局 Shader 参数。

面试表达：除功能正确外，也处理了音频叠加、暂停恢复和表现层资源状态回收。

## 当前设计取舍

### 优点

- 核心职责划分清晰，脚本可按玩家、敌人、战斗、关卡和系统定位。
- 大部分状态通过显式布尔值和协程推进，适合小型 Unity Demo 快速迭代。
- 对暂停、重复触发、重复死亡和攻击重入做了保护。

### 可继续改进

1. 将刷怪、敌人和掉落统一接入对象池，减少高等级时的实例化峰值。
2. 用 `IDamageable` 或 `DamageReceiver` 统一不同敌人的受击接口。
3. 将关卡、敌人和进化参数迁移到 ScriptableObject，减少 Inspector 硬引用。
4. 用事件或服务注入替代 `GameObject.Find`、Tag 查找和单例耦合。
5. 补充 EditMode / PlayMode 测试，覆盖升级、治疗、死亡和 Boss 状态转换。

## 避免夸大的表述

应聘时建议说：

> 这个项目是我独立完成核心玩法的 Unity 2D 生存 Demo。场景源码丢失后，我从构建产物恢复了脚本，
> 重新整理了职责、补充了注释并修复了部分兼容性问题。当前展示的是核心代码与架构，场景资源没有完整恢复。

不要声称恢复结果是未经修改的原始工程，也不要把尚未完成的对象池或测试框架写成已实现功能。
