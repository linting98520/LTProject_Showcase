using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Danmaku.Combat;
using Danmaku.HybridBridge;

namespace Danmaku.Spawning
{
    // Template-method base. Subclasses implement only SetComponent (their unique
    // configuration); the base owns the shared "spawn + bind to cell" flow.
    //
    // Placing a tower is a low-frequency player action, so EntityManager (which
    // returns a real entity we can immediately read back) is the right tool here
    // -- unlike the high-frequency bullet path, which uses ECB inside Burst jobs.
    public abstract class ShooterSpawnerBase
    {
        // SpawnRegistry is a bake-time-fixed singleton and never changes at
        // runtime, so the query is built once and reused rather than re-created
        // on every spawn.
        private EntityManager _manager;
        private EntityQuery   _registryQuery;
        private bool          _initialized;

        private void EnsureInitialized()
        {
            if (_initialized) return;
            _manager       = World.DefaultGameObjectInjectionWorld.EntityManager;
            _registryQuery = _manager.CreateEntityQuery(typeof(SpawnRegistry));
            _initialized   = true;
        }

        public void Spawn(int cellId, Vector3 worldPos, float scale, ShooterBaseData data)
        {
            EnsureInitialized();

            SpawnRegistry config = _registryQuery.GetSingleton<SpawnRegistry>();
            Entity entity = SetComponent(_manager, config, worldPos, scale, data);

            // Links the ECS entity back to its board cell for the hybrid bridge.
            _manager.AddComponentData(entity, new GameObjectLink
            {
                LinkedInstanceID = cellId,
                Type             = LinkType.BuildingCell
            });
        }

        // Read-modify-write keeps the prefab's baked scale intact
        // (LocalTransform.FromPosition would reset Scale to 1).
        protected void SetSpawnPos(EntityManager manager, Entity entity, Vector3 worldPos)
        {
            LocalTransform lt = manager.GetComponentData<LocalTransform>(entity);
            lt.Position = worldPos;
            manager.SetComponentData(entity, lt);
        }

        public abstract Entity SetComponent(EntityManager manager, SpawnRegistry config,
                                            Vector3 worldPos, float scale, ShooterBaseData data);
    }

    public class RadialSpawner : ShooterSpawnerBase
    {
        public override Entity SetComponent(EntityManager manager, SpawnRegistry config,
                                            Vector3 worldPos, float scale, ShooterBaseData data)
        {
            var rd = (RadialShooterData)data;
            Entity shooter = manager.Instantiate(config.RadialShooterEntity);
            SetSpawnPos(manager, shooter, worldPos);

            manager.SetComponentData(shooter, new RadialShooterConfig
            {
                Prefab                 = config.RadialBulletEntity,
                ShooterPosition        = worldPos,
                EmissionDirectionCount = rd.DirCount,
                FireRate               = rd.FireRate,
                ElapsedTime            = rd.FireRate,
                Speed                  = rd.MoveSpeed,
                BulletDamage           = rd.BulletDamage,
                BulletLifetime         = rd.BulletLifeTime
            });
            manager.SetComponentData(shooter, new HealthData { Life = rd.Life });
            return shooter;
        }
    }

    public class OrbitSpawner : ShooterSpawnerBase
    {
        public override Entity SetComponent(EntityManager manager, SpawnRegistry config,
                                            Vector3 worldPos, float scale, ShooterBaseData data)
        {
            var od = (OrbitShooterData)data;
            Entity shooter = manager.Instantiate(config.OrbitShooterEntity);
            SetSpawnPos(manager, shooter, worldPos);

            manager.SetComponentData(shooter, new OrbitShooterConfig
            {
                Prefab                 = config.OrbitBulletEntity,
                ShooterPosition        = worldPos,
                EmissionDirectionCount = od.DirCount,
                ObjectCount            = od.ObjectCount,
                Speed                  = od.RotateSpeed,
                BulletDamage           = od.BulletDamage,
                BulletLifetime         = od.BulletLifeTime
            });
            manager.SetComponentData(shooter, new HealthData { Life = od.Life });
            return shooter;
        }
    }

    public class BlockSpawner : ShooterSpawnerBase
    {
        public override Entity SetComponent(EntityManager manager, SpawnRegistry config,
                                            Vector3 worldPos, float scale, ShooterBaseData data)
        {
            Entity block = manager.Instantiate(config.BlockEntity);
            SetSpawnPos(manager, block, worldPos);

            manager.SetComponentData(block, new BlockData { SpawnPos = worldPos });
            manager.SetComponentData(block, new HealthData { Life = data.Life });
            return block;
        }
    }

    // Maps a ScriptableObject data type to the spawner that builds it.
    // Adding a shooter kind = one data class + one spawner + one dictionary line.
    public class ShooterFactory
    {
        private readonly Dictionary<Type, ShooterSpawnerBase> _spawners = new()
        {
            { typeof(RadialShooterData), new RadialSpawner() },
            { typeof(OrbitShooterData),  new OrbitSpawner()  },
            { typeof(ShooterBaseData),   new BlockSpawner()  },
        };

        public void Spawn(int cellId, Vector3 worldPos, float scale, ShooterBaseData data)
        {
            if (_spawners.TryGetValue(data.GetType(), out var spawner))
                spawner.Spawn(cellId, worldPos, scale, data);
        }
    }
}
