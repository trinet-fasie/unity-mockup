﻿using Entitas;

namespace TM.ECS.Components.Events
{
    public sealed class LoadObjectsCounter : IComponent
    {
        public int PrefabsCount;
        public int PrefabsLoaded;
        public bool LoadComplete;
    }
}
