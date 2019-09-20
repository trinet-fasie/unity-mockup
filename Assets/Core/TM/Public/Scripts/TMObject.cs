using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TM.Public
{
    public class TMObject : MonoBehaviour, IWrapperAware
    {
        private Wrapper _wrapper;
        public Wrapper Wrapper() => _wrapper ?? (_wrapper = CreateInstanceWrapper());

        private Wrapper CreateInstanceWrapper()
        {
            TMObjectDescriptor objectDescriptor = GetComponent<TMObjectDescriptor>();

            string wrapperClassName = objectDescriptor.Name + "Wrapper";

            Assembly objectAssembly = Assembly.GetAssembly(GetType());

            if (objectAssembly == null)
            {
                Debug.LogError("Can't load assembly for: " + GetType().FullName);

                return null;
            }

            Type wrapperType = objectAssembly.GetType("TM.Types." + objectDescriptor.Name + "_" + objectDescriptor.Guid.Replace("-", "") + "." + wrapperClassName);

            if (wrapperType == null)
            {
                return new NullWrapper(gameObject);
            }

            return (Wrapper)Activator.CreateInstance(wrapperType, new object[] {gameObject});
        }

        public bool IsObjectEnabled
        {
            set
            {
                _wrapper.Enabled = value;
            }
            get
            {
                return _wrapper.Enabled;
            }
        }

        public void EnableObject()
        {
            _wrapper.Enable();
        }

        public void DisableObject()
        {
            _wrapper.Disable();
        }

        public void EnableGrab()
        {
            this.EnableInputGrab(gameObject);
        }

        public void DisableGrab()
        {
            this.DisableInputGrab(gameObject);
        }

        public void EnableUse()
        {
            this.EnableInputUsing(gameObject);
        }

        public void DisableUse()
        {
            this.DisableInputUsing(gameObject);
        }

        public void EnableTouch()
        {
            this.EnableTouch(gameObject);
        }

        public void DisableTouch()
        {
            this.DisableTouch(gameObject);
        }

        public void VibrateWithObject(
            GameObject go,
            GameObject controllerObject,
            float strength,
            float duration,
            float interval)
        {
            _wrapper.VibrateWithObject(go, controllerObject, strength, duration, interval);
        }
    }
}