using Unity.Entities;
using Unity.Mathematics;

namespace Danmaku.Spawning
{
    /// <summary> Runtime config for a continuously-firing radial emitter. </summary>
    public struct RadialShooterConfig : IComponentData
    {
        public Entity Prefab;
        public float3 ShooterPosition;
        public int    EmissionDirectionCount;
        public float  FireRate;
        public float  ElapsedTime;
        public float  Speed;
        public float  BulletDamage;
        public float  BulletLifetime;
    }

    /// <summary> Runtime config for a one-shot orbital emitter. </summary>
    public struct OrbitShooterConfig : IComponentData
    {
        public Entity Prefab;
        public float3 ShooterPosition;
        public int    EmissionDirectionCount;
        public int    ObjectCount;
        public float  Speed;
        public float  BulletDamage;
        public float  BulletLifetime;
    }

    /// <summary> Static obstacle marker. </summary>
    public struct BlockData : IComponentData
    {
        public float3 SpawnPos;
    }
}
