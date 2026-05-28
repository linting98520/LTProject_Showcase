using Unity.Burst;
using Unity.Entities;
using Danmaku.Combat;

namespace Danmaku.HybridBridge
{
    // Generic bridge: knows nothing about cells, HUDs or trails. Any entity with
    // both HealthData and GameObjectLink that reaches zero life emits an event
    // before HealthDeathSystem reclaims it. Adding a new link kind requires no
    // change here at all.
    [BurstCompile]
    [UpdateBefore(typeof(HealthDeathSystem))]
    public partial struct GameObjectLinkBrokenDetectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<GameObjectLink>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged)
                               .AsParallelWriter();

            state.Dependency = new DetectionJob { Ecb = ecb }
                              .ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct DetectionJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;

        private void Execute([ChunkIndexInQuery] int sortKey,
                             in HealthData hp,
                             in GameObjectLink link)
        {
            if (hp.Life > 0f) return;

            Entity evt = Ecb.CreateEntity(sortKey);
            Ecb.AddComponent(sortKey, evt, new EntityLinkBrokenEvent
            {
                LinkedInstanceID = link.LinkedInstanceID,
                Type             = link.Type
            });
        }
    }
}
