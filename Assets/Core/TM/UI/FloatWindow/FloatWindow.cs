using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TM.UI
{
    public abstract class FloatWindow : MonoBehaviour
    {
        private Transform head => GameObjects.Instance.Head;
        
        protected void UpdateWindowPosition()
        {
            float angleDiff = Quaternion.FromToRotation(-transform.forward, head.forward).eulerAngles.y;
            float heightDiff = transform.position.y - head.transform.position.y + 0.7f;

            if (angleDiff >= 180)
            {
                angleDiff = angleDiff - 360;
            }

            float angleThreshold = 25;

            if (Mathf.Abs(angleDiff) >= angleThreshold)
            {
                float rotationSpeed = 2 * angleThreshold * Mathf.Sign(angleDiff);

                if (Mathf.Abs(angleDiff) > angleThreshold * 3)
                {
                    rotationSpeed *= 5;
                }

                transform.RotateAround(head.position, Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            float heightThreshold = 0.2f;
            
            if (Mathf.Abs(heightDiff) >= heightThreshold)
            {
                float moveSpeed = 0.6f;
                
                if (heightDiff < 0)
                {
                    transform.position += Vector3.up * Time.deltaTime * moveSpeed;
                }
                else
                {
                    transform.position -= Vector3.up * Time.deltaTime * moveSpeed;
                }
               
            }
            
        }
        
        protected void SetWindowPosition()
        {
            transform.SetParent(null);

            Vector3 headXZForward = head.forward;
            headXZForward.y = 0;
            headXZForward.Normalize();

            transform.position = head.position + 2 * headXZForward - 0.7f * Vector3.up;

            transform.LookAt(head);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            transform.SetParent(head.parent);
        }
    }
}