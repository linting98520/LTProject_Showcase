using Unity.Entities;
using UnityEngine;
using Danmaku.Movement;
using Danmaku.Combat;

namespace Danmaku.Authoring
{
    /// <summary>
    /// Bakes a bullet GameObject prefab into an entity carrying every component
    /// the movement/hit pipeline expects. Structure is decided here at author
    /// time; spawners only set values at runtime (no structural changes per shot).
    /// </summary>
    public class BulletAuthoring : MonoBehaviour
    {
        public float Speed    = 8f;
        public float Damage   = 10f;
        public float Lifetime = 3f;

        private class Baker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new LinearMoveData { Speed = authoring.Speed });
                AddComponent<NextPosition>(entity);
                AddComponent(entity, new Damage { Value = authoring.Damage });
                AddComponent(entity, new ProjectileLifeTimeData { RemainingTime = authoring.Lifetime });
            }
        }
    }
}
