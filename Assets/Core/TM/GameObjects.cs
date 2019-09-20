using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using TM.UI;
using VRTK;

namespace TM
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class GameObjects : MonoBehaviour
    {
        public static GameObjects Instance;

        #region Player Transforms

        public Transform PlayerRig;
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public Transform TipAttach;
        #endregion

        #region UI prefabs

        public GameObject UIID;
        public GameObject UIObject;
        public GameObject UIToolTip;
        public GameObject Load;

        #endregion

        public Dictionary<string, GameObject> MagnetObjects = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Instance = this;
            
        }
        
        private void Start()
        {
            StartCoroutine(WaitPointer());
        }

        private IEnumerator WaitPointer()
        {
            VRTK_StraightPointerRenderer vrtkPointer = null;

            while (vrtkPointer == null)
            {
                yield return new WaitForEndOfFrame();
                vrtkPointer = RightHand.GetComponentInChildren<VRTK_StraightPointerRenderer>();
            }

            yield return true;
        }


    }
}

  