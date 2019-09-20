namespace TM.UI.VRErrorManager
{
    public class UIExitButton : UIButton
    {
        public override void OnClick()
        {
            VRErrorManager.Instance.Exit();
        }
    }
}
