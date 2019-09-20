using UnityEngine;
using TM.Models.Data;

namespace TM
{
    public class ObjectTransform : MonoBehaviour
    {
        public TransformDto GetTransform()
        {
            var objectId = gameObject.GetComponent<ObjectId>();
            if (objectId == null) return null;

            TransformDT transformDt = gameObject.transform.ToTransformDT();
            return new TransformDto
            {
                Id = objectId.Id,
                Transform = transformDt
            };
        }
    }
}
