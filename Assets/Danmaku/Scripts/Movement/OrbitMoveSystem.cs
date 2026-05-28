using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;

namespace Danmaku.Movement
{
    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    [UpdateBefore(typeof(HitMoveSystem))]
    public partial struct OrbitMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<OrbitMoveData>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new OrbitMoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct OrbitMoveJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(ref OrbitMoveData orbit, ref NextPosition next)
        {
            // Resolve the position for the current angle first, then advance,
            // so the very first frame lands exactly on the spawn angle.
            float x = math.cos(orbit.Angle) * orbit.Radius;
            float z = math.sin(orbit.Angle) * orbit.Radius;
            next.Value = orbit.Center + new float3(x, 0f, z);

            orbit.Angle += orbit.Speed * DeltaTime;
        }
    }
}
