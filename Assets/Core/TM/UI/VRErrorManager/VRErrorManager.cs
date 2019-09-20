using System;
using SmartLocalization;
using SmartLocalization.Editor;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

namespace TM.UI.VRErrorManager
{
    /// <summary>
    /// Temp class! Wil be remaked!
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class VRErrorManager : MonoBehaviour
    {
        public Vector3 LocalPositionOnHead;
        public GameObject Panel;
        public GameObject PanelFatalError;
        public GameObject ExitButton;
        public GameObject ExitButtonLabel;
        public Text Message;
        public LocalizedText TryAgainOrSend;
        private Action _reTry;
        public static VRErrorManager Instance;
        private bool fatalError = false;
        public bool Showing => Panel.activeSelf;

        private void Awake()
        {
            Instance = this;
            //LanguageManager.Instance.ChangeLanguage(Settings.Instance().Language);
        }

        public void Show(string message, Action retry = null)
        {
            NotificationWindowManager.Show(message, 5.0f);
        }

        public void ShowFatal(string message, string stackTrace = null)
        {
            fatalError = true;
            TryAgainOrSend.localizedKey = "SEND_REPORT";
            Message.text = message;
            Message.color = Color.red;
            PanelFatalError.SetActive(true);
            _reTry = Application.Quit;
            Panel.SetActive(true);
            ExitButton.SetActive(true);
            ExitButtonLabel.SetActive(true);
            
            VRTK_CustomRaycast.MenuMode = true;
        }

        public void Hide()
        {
            Panel.SetActive(false);
            VRTK_CustomRaycast.MenuMode = false;
        }

        public void ReTryOrSendReport()
        {
            _reTry?.Invoke();
            if (!fatalError) Hide();
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}
