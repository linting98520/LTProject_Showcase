using UnityEngine;

namespace Danmaku.Spawning
{
    /// <summary>
    /// Designer-facing balance data. One prefab + many ScriptableObject variants
    /// produces many tower behaviours without authoring extra prefabs.
    /// </summary>
    public abstract class ShooterBaseData : ScriptableObject
    {
        public int   Id;
        public float Life = 50f;
    }

    [CreateAssetMenu(menuName = "Danmaku/Radial Shooter")]
    public class RadialShooterData : ShooterBaseData
    {
        public int   DirCount       = 8;
        public float FireRate       = 1f;
        public float MoveSpeed      = 8f;
        public float BulletDamage   = 10f;
        public float BulletLifeTime = 3f;
    }

    [CreateAssetMenu(menuName = "Danmaku/Orbit Shooter")]
    public class OrbitShooterData : ShooterBaseData
    {
        public int   DirCount       = 8;
        public int   ObjectCount    = 3;
        public float RotateSpeed    = 2f;
        public float BulletDamage   = 10f;
        public float BulletLifeTime = 5f;
    }
}
