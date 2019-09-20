// Snap Rotate Object Control Action|ObjectControlActions|25030

using System;
using System.Collections;
using TM;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;

//TM: custom snap rotation script which takes into account the VR device 
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// The Snap Rotate Object Control Action script is used to snap rotate the controlled GameObject around the up vector when changing the axis.
    /// </summary>
    /// <remarks>
    /// The effect is a immediate snap rotation to quickly face in a new direction.
    /// </remarks>
    /// <example>
    /// `VRTK/Examples/017_CameraRig_TouchpadWalking` has a collection of walls and slopes that can be traversed by the user with the touchpad. There is also an area that can only be traversed if the user is crouching.
    ///
    /// To enable the Snap Rotate Object Control Action, ensure one of the `TouchpadControlOptions` children (located under the Controller script alias) has the `Snap Rotate` control script active.
    /// </example>
    [AddComponentMenu("VRTK/Scripts/Locomotion/Object Control Actions/VRTK_SnapRotateObjectControlActionOW")]
    public class VRTK_SnapRotateObjectControlActionOW : VRTK_BaseObjectControlAction
    {
        [Tooltip("The angle to rotate for each snap.")]
        public float anglePerSnap = 45f;
        [Tooltip("The speed for the headset to fade out and back in. Having a blink between rotations can reduce nausia.")]
        public float blinkTransitionSpeed = 0.0f;
        [Range(0, 1f)]
        [Tooltip("The threshold the listened axis needs to exceed before the action occurs. This can be used to limit the snap rotate to a single axis direction (e.g. pull down to flip rotate). The threshold is ignored if it is 0.")]
        public float axisThreshold = 0.7f;

        [Range(0, 1f)]
        public float axisLowerThreshold = 0.3f;

        public Transform PlayerRig;

        public VRTK_Pointer Pointer;

        private bool _axisReturnedToDefault = true;

        protected override void Process(GameObject controlledGameObject, Transform directionDevice, Vector3 axisDirection, float axis, float deadzone, bool currentlyFalling, bool modifierActive)
        {
            
            if (Mathf.Abs(axis) < axisLowerThreshold  || (!modifierActive && !XRDevice.model.Contains("Oculus")))
            {
                _axisReturnedToDefault = true;
            }

            if (Mathf.Abs(axis) < axisThreshold)
            {
                Pointer.enableTeleport = true;               
            }
            else
            {
                Pointer.enableTeleport = false;
            }
            
            if (Mathf.Abs(axis) < axisThreshold)
            {
                Pointer.overThreshold = false;
            }
            else if (Mathf.Abs(axis) > axisThreshold * 0.9f)
            {
                Pointer.overThreshold = true;
            }

            if (!ValidThreshold(axis))
            {
                return;
            }

            if (!modifierActive && !XRDevice.model.Contains("Oculus"))
            {
                return;
            }

            float angle = Rotate(axis, false);

            if (angle != 0f)
            {
                Blink(blinkTransitionSpeed);
                controlledGameObject.transform.RotateAround(directionDevice.position, Vector3.up, angle);

                _axisReturnedToDefault = false;    
                Pointer.Toggle(false);
            }          
        }

        protected virtual bool ValidThreshold(float axis)
        {
            return (Mathf.Abs(axis)>=axisThreshold && _axisReturnedToDefault);
        }

        protected virtual float Rotate(float axis, bool modifierActive)
        {
            int directionMultiplier = GetAxisDirection(axis);
            return (anglePerSnap) * directionMultiplier;
        }
    }
}