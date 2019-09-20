using TM.UI.VRMessageManager;

namespace TM.UI.VRErrorManager
{
    public class UIDialogResultButton : UIButton
    {
        public DialogResult Result;
        public override void OnClick()
        {
            VRMessageManager.VRMessageManager.Instance.SetResult(Result);
        }
    }
}
