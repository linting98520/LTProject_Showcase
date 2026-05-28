using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Danmaku.Combat;

namespace Danmaku.Movement
{
    // The single source of truth for projectile collision + committed movement.
    // Casting a ray along (currentPos -> intendedPos) makes the check immune to
    // tunnelling regardless of projectile speed. Every movement rule funnels
    // through here, so collision logic is written exactly once.
    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial struct HitMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<NextPosition>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            var filter = new CollisionFilter
            {
                BelongsTo    = 1u << 8,                 // projectile layer
                CollidesWith = (1u << 0) | (1u << 9),   // world + target layers
                GroupIndex   = 0
            };

            state.Dependency = new HitMoveJob
            {
                PhysicsWorld = physicsWorld,
                Ecb          = ecb,
                Filter       = filter
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct HitMoveJob : IJobEntity
    {
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public CollisionFilter Filter;

        // Requiring `in Damage` scopes this job to damage-dealing projectiles.
        private void Execute([ChunkIndexInQuery] int sortKey,
                             Entity entity,
                             ref LocalTransform transform,
                             in NextPosition next,
                             in Damage damage)
        {
            var ray = new RaycastInput
            {
                Start  = transform.Position,
                End    = next.Value,
                Filter = Filter
            };

            if (PhysicsWorld.CastRay(ray, out var hit))
            {
                // Decouple damage from collision via a one-frame event entity.
                Entity evt = Ecb.CreateEntity(sortKey);
                Ecb.AddComponent(sortKey, evt, new DamageEvent
                {
                    Target = hit.Entity,
                    Amount = damage.Value
                });
                Ecb.DestroyEntity(sortKey, entity);
                return;
            }

            transform.Position = next.Value;   // commit the move only if clear
        }
    }
}
