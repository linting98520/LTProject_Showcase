using Unity.Burst;
using Unity.Entities;

namespace Danmaku.Combat
{
    // Each entity inspects only its own health, so death detection is trivially
    // parallel.
    [BurstCompile]
    [UpdateAfter(typeof(DamageApplySystem))]
    public partial struct HealthDeathSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<HealthData>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            state.Dependency = new HealthDeathJob { Ecb = ecb }
                              .ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct HealthDeathJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int sortKey, Entity entity, in HealthData hp)
        {
            if (hp.Life <= 0f)
                Ecb.DestroyEntity(sortKey, entity);
        }
    }
}
