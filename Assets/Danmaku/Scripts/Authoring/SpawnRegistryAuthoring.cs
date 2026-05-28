using Unity.Entities;
using UnityEngine;

namespace Danmaku.Authoring
{
    /// <summary>
    /// Collects all spawnable prefabs into a single SpawnRegistry singleton.
    /// Designers drag prefabs into the inspector; the baker converts each into a
    /// prefab-entity reference. Adding a new prefab = one field + one line below.
    /// </summary>
    public class SpawnRegistryAuthoring : MonoBehaviour
    {
        public GameObject RadialBulletPrefab;
        public GameObject OrbitBulletPrefab;
        public GameObject BlockPrefab;
        public GameObject RadialShooterPrefab;
        public GameObject OrbitShooterPrefab;

        private class Baker : Baker<SpawnRegistryAuthoring>
        {
            public override void Bake(SpawnRegistryAuthoring a)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new Spawning.SpawnRegistry
                {
                    RadialBulletEntity  = GetEntity(a.RadialBulletPrefab,  TransformUsageFlags.Dynamic),
                    OrbitBulletEntity   = GetEntity(a.OrbitBulletPrefab,   TransformUsageFlags.Dynamic),
                    BlockEntity         = GetEntity(a.BlockPrefab,         TransformUsageFlags.Dynamic),
                    RadialShooterEntity = GetEntity(a.RadialShooterPrefab, TransformUsageFlags.Dynamic),
                    OrbitShooterEntity  = GetEntity(a.OrbitShooterPrefab,  TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}
