using Unity.Entities;

namespace Danmaku.HybridBridge
{
    /// <summary> Shared tag identifying what kind of GameObject an entity maps to. </summary>
    public enum LinkType : byte
    {
        None         = 0,
        BuildingCell = 1,   // shooter -> board cell
        HudBar       = 2,   // enemy   -> health-bar UI
        Trail        = 3,   // bullet  -> trail renderer
        AudioSource  = 4,
    }

    /// <summary> Lives on any entity that mirrors a GameObject. </summary>
    public struct GameObjectLink : IComponentData
    {
        public int      LinkedInstanceID;   // GameObject.GetInstanceID()
        public LinkType Type;
    }

    /// <summary> Emitted when a linked entity is destroyed, so the GO side can react. </summary>
    public struct EntityLinkBrokenEvent : IComponentData
    {
        public int      LinkedInstanceID;
        public LinkType Type;
    }
}
