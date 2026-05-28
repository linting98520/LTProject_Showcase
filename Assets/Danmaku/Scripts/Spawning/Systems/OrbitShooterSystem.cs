using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Danmaku.Spawning
{
    [BurstCompile]
    public partial struct OrbitShooterSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<OrbitShooterConfig>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            state.Dependency = new OrbitShooterSpawnJob { Ecb = ecb }
                              .ScheduleParallel(state.Dependency);
        }
    }

    // One-shot emitter: spawns its full ring of orbiting bullets once, then
    // removes its own config so the query no longer matches it. The shooter
    // entity itself (the visible tower) is preserved.
    [BurstCompile]
    public partial struct OrbitShooterSpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute(Entity entity,
                             [ChunkIndexInQuery] int sortKey,
                             in OrbitShooterConfig config)
        {
            var bulletParams = new BulletSpawnParams
            {
                Prefab   = config.Prefab,
                Speed    = config.Speed,
                Damage   = config.BulletDamage,
                Lifetime = config.BulletLifetime
            };

            float angleStep = math.PI * 2f / config.EmissionDirectionCount;
            for (int i = 0; i < config.EmissionDirectionCount; i++)
            {
                float angle = i * angleStep;
                for (int ring = 0; ring < config.ObjectCount; ring++)
                {
                    float radius = (ring + 1) * 2f;
                    BulletSpawnHelper.SpawnOrbitBullet(
                        ref Ecb, sortKey, in bulletParams, config.ShooterPosition, radius, angle);
                }
            }

            Ecb.RemoveComponent<OrbitShooterConfig>(sortKey, entity);
        }
    }
}
