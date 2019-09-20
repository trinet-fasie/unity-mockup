using UnityEngine;

namespace TM
{
    public class ObjectPointerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private InputController _inputController;

        public void Init(InputController inputController)
        {
            _inputController = inputController;
        }

        public void OnHover()
        {
            _inputController.OnPointerIn?.Invoke();
        }

        public void OnOut()
        {
            _inputController.OnPointerOut?.Invoke();
        }

        public void OnClick()
        {
            _inputController.OnPointerClick?.Invoke();
        }

    }
}
