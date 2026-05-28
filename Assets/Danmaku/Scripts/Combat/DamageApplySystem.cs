using Unity.Entities;
using Danmaku.Movement;

namespace Danmaku.Combat
{
    // Applies queued DamageEvents on the main thread. A ComponentLookup write
    // from a parallel job would race when two projectiles hit the same target
    // in one frame; per-frame event counts are small, so the main thread is the
    // safe and simple choice.
    [UpdateAfter(typeof(HitMoveSystem))]
    public partial struct DamageApplySystem : ISystem
    {
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<DamageEvent>();

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged);
            var healthLookup = SystemAPI.GetComponentLookup<HealthData>();

            foreach (var (evt, eventEntity) in
                     SystemAPI.Query<RefRO<DamageEvent>>().WithEntityAccess())
            {
                Entity target = evt.ValueRO.Target;

                // Targets without HealthData (e.g. walls) simply absorb the hit.
                if (healthLookup.HasComponent(target))
                {
                    HealthData hp = healthLookup[target];
                    hp.Life -= evt.ValueRO.Amount;
                    healthLookup[target] = hp;
                }

                ecb.DestroyEntity(eventEntity);
            }
        }
    }
}
