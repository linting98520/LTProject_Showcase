using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Danmaku.Movement;
using Danmaku.Combat;

namespace Danmaku.Spawning
{
    /// <summary> Parameters shared by every bullet, regardless of movement rule. </summary>
    public struct BulletSpawnParams
    {
        public Entity Prefab;
        public float  Speed;
        public float  Damage;
        public float  Lifetime;
    }

    /// <summary>
    /// Pure, stateless, Burst-compatible helper that configures a single bullet.
    /// Spawners call into this instead of duplicating the SetComponent block, so
    /// a new per-bullet field is added in exactly one place.
    /// </summary>
    [BurstCompile]
    public static class BulletSpawnHelper
    {
        [BurstCompile]
        public static Entity SpawnLinearBullet(
            ref EntityCommandBuffer.ParallelWriter ecb,
            int sortKey,
            in BulletSpawnParams p,
            float3 position,
            float3 direction)
        {
            Entity bullet = ecb.Instantiate(sortKey, p.Prefab);
            quaternion rotation = quaternion.LookRotationSafe(direction, math.up());

            ecb.SetComponent(sortKey, bullet, LocalTransform.FromPositionRotation(position, rotation));
            ecb.SetComponent(sortKey, bullet, new LinearMoveData { Direction = direction, Speed = p.Speed });
            ecb.SetComponent(sortKey, bullet, new NextPosition { Value = position });   // prevents 1st-frame teleport to origin
            ecb.SetComponent(sortKey, bullet, new Damage { Value = p.Damage });
            ecb.SetComponent(sortKey, bullet, new ProjectileLifeTimeData { RemainingTime = p.Lifetime });
            return bullet;
        }

        [BurstCompile]
        public static Entity SpawnOrbitBullet(
            ref EntityCommandBuffer.ParallelWriter ecb,
            int sortKey,
            in BulletSpawnParams p,
            float3 center,
            float radius,
            float startAngle)
        {
            Entity bullet = ecb.Instantiate(sortKey, p.Prefab);

            float3 initialPos = center + new float3(
                math.cos(startAngle) * radius, 0f, math.sin(startAngle) * radius);

            ecb.SetComponent(sortKey, bullet, LocalTransform.FromPosition(initialPos));
            ecb.SetComponent(sortKey, bullet, new OrbitMoveData
            {
                Center = center, Radius = radius, Speed = p.Speed, Angle = startAngle
            });
            ecb.SetComponent(sortKey, bullet, new NextPosition { Value = initialPos });
            ecb.SetComponent(sortKey, bullet, new Damage { Value = p.Damage });
            ecb.SetComponent(sortKey, bullet, new ProjectileLifeTimeData { RemainingTime = p.Lifetime });
            return bullet;
        }
    }
}
