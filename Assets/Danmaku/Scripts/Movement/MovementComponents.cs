using Unity.Entities;
using Unity.Mathematics;

namespace Danmaku.Movement
{
    /// <summary>
    /// The position this entity intends to reach this frame.
    /// Movement systems WRITE this; HitMoveSystem READS it and decides whether
    /// the move is actually allowed. Separating "intent" from "commit" is what
    /// lets a single collision system serve every movement rule.
    /// </summary>
    public struct NextPosition : IComponentData
    {
        public float3 Value;
    }

    /// <summary> Straight-line movement parameters. </summary>
    public struct LinearMoveData : IComponentData
    {
        public float3 Direction;   // normalised
        public float  Speed;
    }

    /// <summary> Circular-orbit movement parameters. </summary>
    public struct OrbitMoveData : IComponentData
    {
        public float3 Center;
        public float  Radius;
        public float  Speed;       // angular, rad/s
        public float  Angle;       // current angle, rad
    }

    /// <summary> Remaining lifetime; entity is recycled when it hits zero. </summary>
    public struct ProjectileLifeTimeData : IComponentData
    {
        public float RemainingTime;
    }
}
