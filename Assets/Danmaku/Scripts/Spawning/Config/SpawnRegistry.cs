using Unity.Entities;

namespace Danmaku.Spawning
{
    /// <summary>
    /// Single holder for every baked prefab entity. A spawner reads it once
    /// (cached query) instead of looking prefabs up individually. Adding a new
    /// prefab is a one-field change here plus one line in its Authoring baker.
    /// </summary>
    public struct SpawnRegistry : IComponentData
    {
        public Entity RadialBulletEntity;
        public Entity OrbitBulletEntity;
        public Entity BlockEntity;

        public Entity RadialShooterEntity;
        public Entity OrbitShooterEntity;
    }
}
