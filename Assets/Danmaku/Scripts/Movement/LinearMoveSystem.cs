using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Danmaku.Movement
{
    // Runs inside the physics group, after the simulation, and BEFORE the hit
    // system. [UpdateBefore] only orders systems within the SAME group, so this
    // system must live in PhysicsSystemGroup alongside HitMoveSystem.
    [BurstCompile]
    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    [UpdateBefore(typeof(HitMoveSystem))]
    public partial struct LinearMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) =>
            state.RequireForUpdate<LinearMoveData>();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Explicit dependency hand-off: required because the job writes
            // LocalTransform, which the physics integrity check also reads.
            state.Dependency = new LinearMoveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct LinearMoveJob : IJobEntity
    {
        public float DeltaTime;

        // Query is auto-derived from the signature: only entities owning all
        // three components are processed. No per-entity branching needed.
        private void Execute(in LocalTransform transform,
                             in LinearMoveData move,
                             ref NextPosition next)
        {
            next.Value = transform.Position + move.Direction * move.Speed * DeltaTime;
        }
    }
}
