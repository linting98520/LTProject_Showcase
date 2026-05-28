using Unity.Burst;
using Unity.Entities;

namespace Danmaku.Movement
{
    // Counts down each projectile's lifetime and recycles it on expiry.
    // Trivially parallel: every entity touches only its own data.
    [BurstCompile]
    public partial struct ProjectileLifeTimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<ProjectileLifeTimeData>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            state.Dependency = new LifeTimeJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb       = ecb
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct LifeTimeJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int sortKey,
                             Entity entity,
                             ref ProjectileLifeTimeData life)
        {
            life.RemainingTime -= DeltaTime;
            if (life.RemainingTime <= 0f)
                Ecb.DestroyEntity(sortKey, entity);
        }
    }
}
