using Unity.Entities;

namespace Danmaku.Combat
{
    /// <summary> Damage carried by a projectile. </summary>
    public struct Damage : IComponentData
    {
        public float Value;
    }

    /// <summary> Health on anything that can be hurt. </summary>
    public struct HealthData : IComponentData
    {
        public float Life;
    }

    /// <summary>
    /// One-frame event entity describing "apply N damage to target".
    /// Produced by the hit job, consumed by DamageApplySystem. Using an event
    /// entity avoids cross-entity writes from a parallel job (a data race) and
    /// keeps damage modifiers (crit, resist, knockback) in one place.
    /// </summary>
    public struct DamageEvent : IComponentData
    {
        public Entity Target;
        public float  Amount;
    }
}
