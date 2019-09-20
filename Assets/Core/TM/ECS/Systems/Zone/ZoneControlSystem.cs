using System;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace TM.ECS.Systems
{
    public sealed class ZoneControlSystem : IExecuteSystem, ICleanupSystem
    {
        private readonly IGroup<GameEntity> _zoneEntities;
        private readonly IGroup<GameEntity> _allEntites;
        private List<Wrapper> _overlappingObjects;

        public ZoneControlSystem(Contexts contexts)
        {
            _zoneEntities = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Zone, GameMatcher.GameObject,
                GameMatcher.ColliderAware));
            _allEntites = contexts.game.GetGroup(GameMatcher.AllOf(GameMatcher.Wrapper, GameMatcher.Collider));
        }

        public void Execute()
        {
            return;
        }


        public void Cleanup()
        {
        }
    }
}