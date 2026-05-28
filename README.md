# 架構精選

玩家在格子上放置「發射器塔」，塔以 ECS 大量生成子彈；
子彈、敵人、傷害結算全部跑在 DOTS（資料導向）架構上以支撐高數量同屏實體，
而 UI、棋盤格子等互動物件維持 GameObject——因此核心挑戰之一是**兩種範式之間的乾淨橋接**。

---

## 架構總覽

[Movement 階段 — 各 MoveSystem 平行，互不依賴]
   LinearMoveSystem, OrbitMoveSystem  ->  寫入 NextPosition（「這一幀想去哪」）

[Hit 階段 — 唯一的碰撞邏輯]
   HitMoveSystem  讀 currentPos -> NextPosition，沿線段 raycast
        命中：產生 DamageEvent + 銷毀子彈
        沒命中：才真正位移 (commit)

[Combat 階段]
   DamageApplySystem  集中套用傷害（主執行緒，避免平行 race）
   HealthDeathSystem  血量歸零 -> 平行回收

[Bridge 階段 — ECS 通知 GameObject]
   GameObjectLinkBrokenDetectionSystem  偵測帶 GameObjectLink 的 entity 死亡 -> 發事件
   LinkBrokenDispatcher (MonoBehaviour)  依 LinkType 分派給對應 handler
```

設計關鍵在於**把「移動意圖」與「移動提交」分離**：移動系統只宣告想去的位置，
真正的位移由唯一的碰撞系統在驗證安全後執行。這讓所有移動規則共用同一套碰撞邏輯，
且天生免疫高速子彈的 tunneling。

---

## 資料夾結構

```
Assets/Danmaku/Scripts/
├── Movement/                 子彈移動 + 碰撞管線
│   ├── MovementComponents.cs        NextPosition / LinearMoveData / OrbitMoveData / ProjectileLifeTimeData
│   ├── LinearMoveSystem.cs          直線移動（寫 NextPosition）
│   ├── OrbitMoveSystem.cs           環繞移動（寫 NextPosition）
│   ├── HitMoveSystem.cs             唯一的線段 raycast 碰撞 + 提交移動
│   └── ProjectileLifeTimeSystem.cs  生命週期回收
│
├── Combat/                   事件式傷害 / 死亡
│   ├── CombatComponents.cs          Damage / HealthData / DamageEvent
│   ├── DamageApplySystem.cs         集中套用傷害
│   └── HealthDeathSystem.cs         死亡回收
│
├── Spawning/                 發射器與子彈生成
│   ├── Config/                      ECS runtime 設定 component + SpawnRegistry
│   ├── Data/                        ScriptableObject 平衡數值（設計師可調）
│   ├── Helpers/BulletSpawnHelper.cs 抽出「設定一顆子彈」的共用原子操作
│   ├── Systems/                     Radial（連發）/ Orbit（一次性）發射器
│   └── Factory/ShooterSpawner.cs    Factory + 模板方法，從 SO 生成塔
│
├── HybridBridge/             ECS <-> GameObject 解耦橋接
│   ├── LinkTypes.cs                 GameObjectLink / EntityLinkBrokenEvent / LinkType
│   ├── GameObjectLinkBrokenDetectionSystem.cs   通用死亡偵測（Burst）
│   └── LinkBrokenDispatcher.cs      主執行緒事件分派
│
└── Authoring/                GameObject -> Entity 烘焙
    ├── BulletAuthoring.cs           結構在編輯期決定，runtime 只改值
    └── SpawnRegistryAuthoring.cs    集中持有所有 prefab 參照
```