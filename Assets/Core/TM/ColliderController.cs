using System.Collections.Generic;
using UnityEngine;
using TM.Public;

namespace TM
{
    public class ColliderController
    {
        private readonly GameEntity _entity;
        public ColliderController(IColliderAware colliderAware, GameObject go)
        {
            _entity = Contexts.sharedInstance.game.CreateEntity();
            _entity.AddGameObject(go);
            _entity.AddZone(new List<Wrapper>());
            _entity.AddColliderAware(colliderAware);
        }

        public void Destroy()
        {
            _entity.Destroy();
        }
         
    }
}
