using UnityEngine;

namespace TM
{
    public class NullWrapper : Wrapper
    {
        public NullWrapper(GameEntity entity) : base(entity)
        {
        }

        public NullWrapper(GameObject gameObject) : base(gameObject)
        {
        }
    }
}
