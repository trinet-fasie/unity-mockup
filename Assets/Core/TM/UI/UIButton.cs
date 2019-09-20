using UnityEngine;

namespace TM.UI
{
    // ReSharper disable once InconsistentNaming
    public abstract class UIButton : MonoBehaviour
    {
        public GameObject OnHoverGo;

        public virtual void OnHover()
        {
            if (OnHoverGo != null)
            {
                OnHoverGo.SetActive(true);
            }
        }

        public virtual void OnOut()
        {
            if (OnHoverGo != null)
            {
                OnHoverGo.SetActive(false);
            }
        }       
        public abstract void OnClick();

    }
}
