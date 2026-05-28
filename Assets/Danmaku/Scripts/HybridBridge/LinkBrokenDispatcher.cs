using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Danmaku.HybridBridge
{
    /// <summary>
    /// Main-thread side of the bridge. Drains EntityLinkBrokenEvents each frame
    /// and routes them to the handler registered for that LinkType. Multiple
    /// handlers per type are supported (multicast), so cross-cutting reactions
    /// -- release the cell, play a sound, grant currency -- subscribe
    /// independently of the ECS world.
    /// </summary>
    public class LinkBrokenDispatcher : MonoBehaviour
    {
        public static LinkBrokenDispatcher Instance { get; private set; }

        private readonly Dictionary<LinkType, Action<int>> _handlers = new();
        private EntityQuery _eventQuery;

        private void Awake()
        {
            Instance = this;
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _eventQuery = em.CreateEntityQuery(typeof(EntityLinkBrokenEvent));
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Register(LinkType type, Action<int> handler)
        {
            // NOTE: Dictionary values are value-typed delegates, so the combined
            // delegate must be written back. Mutating a local copy would silently
            // drop every handler after the first -- a subtle, easy-to-miss bug.
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = existing + handler;
            else
                _handlers[type] = handler;
        }

        public void Unregister(LinkType type, Action<int> handler)
        {
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = existing - handler;
        }

        private void Update()
        {
            if (_eventQuery.IsEmpty) return;

            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var events   = _eventQuery.ToComponentDataArray<EntityLinkBrokenEvent>(Allocator.Temp);
            var entities = _eventQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < events.Length; i++)
            {
                if (_handlers.TryGetValue(events[i].Type, out var handler))
                    handler?.Invoke(events[i].LinkedInstanceID);

                em.DestroyEntity(entities[i]);
            }

            events.Dispose();
            entities.Dispose();
        }
    }
}
