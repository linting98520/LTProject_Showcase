using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Danmaku.Spawning
{
    [BurstCompile]
    public partial struct RadialShooterSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<RadialShooterConfig>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            state.Dependency = new RadialShooterSpawnJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb       = ecb
            }.ScheduleParallel(state.Dependency);
        }
    }

    // A continuously-firing emitter. Each Execute reduces to the emitter's own
    // "personality" (radial distribution + fire-rate timing); per-bullet setup
    // is delegated to BulletSpawnHelper.
    [BurstCompile]
    public partial struct RadialShooterSpawnJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int sortKey, ref RadialShooterConfig config)
        {
            config.ElapsedTime += DeltaTime;
            if (config.ElapsedTime < config.FireRate) return;
            config.ElapsedTime = 0f;

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
                float3 dir  = new float3(math.cos(angle), 0f, math.sin(angle));
                float3 pos  = config.ShooterPosition + dir * 2f;   // offset clears the tower collider

                BulletSpawnHelper.SpawnLinearBullet(ref Ecb, sortKey, in bulletParams, pos, dir);
            }
        }
    }
}
