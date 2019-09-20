using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TM.UI;

namespace TM.UI
{
    public class PopupWindowButton : UIButton
    {
        [SerializeField]
        private Text _buttonText;

        [SerializeField]
        private Button _uiButton;

        public delegate void ButtonClickDelegate();

        public event ButtonClickDelegate ButtonClickedEvent;

        private void Reset()
        {
            _buttonText = GetComponentInChildren<Text>();
            _uiButton = GetComponentInChildren<Button>();
        }

        public override void OnClick()
        {
            ButtonClickedEvent?.Invoke();
        }

        public override void OnHover()
        {
            if (_uiButton != null)
            {
                _uiButton.OnSelect(null);
            }
            else
            {
                base.OnHover();
            }
        }

        public override void OnOut()
        {
            if (_uiButton != null)
            {
                _uiButton.OnDeselect(null);
            }
            else
            {
                base.OnOut();
            }
        }

        public string ButtonText
        {
            set => _buttonText.text = value;
            get => _buttonText.text;
        }
    }
}