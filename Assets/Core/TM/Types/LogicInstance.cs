using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using SmartLocalization;
using TM.Errors;
using TM.UI.VRErrorManager;
using TM.WWW;

namespace TM.Types
{
    public class LogicInstance
    {
        public ILogic Logic { get; private set; }

        private readonly int _worldLocationId;
        private WrappersCollection _myItems;
        private bool _initialized;

        public LogicInstance(int worldLocationId)
        {
            _worldLocationId = worldLocationId;
            GameStateData.SetLogic(this);
        }
        
        public void UpdateGroupLogic(Type newLogic)
        {
            if (newLogic == null)
            {
                LogManager.GetCurrentClassLogger().Info($"Location {_worldLocationId} logic is empty!");

                return;
            }

            _myItems = GameStateData.GetWrapperCollection();
            ILogic logic = Activator.CreateInstance(newLogic) as ILogic;
            

            if (logic == null)
            {
                LogManager.GetCurrentClassLogger().Error($"Initialize location logic error! WorldLocationId = {_worldLocationId}. Message: Logic is null!");
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.LogicInitError));

                return;
            }

            Logic = logic;

            try
            {
                LogManager.GetCurrentClassLogger().Info($"Location {_worldLocationId} logic initialize started...");
                _initialized = false;
                Logic.SetCollection(_myItems);
                Logic.Initialize();
                Logic.Events();
                _initialized = true;
                LogManager.GetCurrentClassLogger().Info($"Location {_worldLocationId} logic initialize successful");
            }
            catch (Exception e)
            {
                ShowLogicExceptionError(Errors.ErrorCode.LogicInitError, "Initialize location logic error!", e);
                Logic = null;
            }
        }

        public void ExecuteLogic()
        {
            if (Logic == null || !_initialized)
            {
                return;
            }

            try
            {
                Logic.Update();
            }
            catch (Exception e)
            {
                ShowLogicExceptionError(Errors.ErrorCode.LogicExecuteError, "Execute location logic error!", e);
                Logic = null;

                
            }
        }

        private void ShowLogicExceptionError(int errorCode, string errorMessage, Exception exception)
        {
            StackTrace st = new StackTrace(exception, true);
            StackFrame frame = st.GetFrame(0);
            MethodBase method = frame.GetMethod();
            string errorMessageToServer = "";
            string typeFullName = "";
            string methodName = "";
            if (method != null && method.DeclaringType != null)
            {
                typeFullName = method.DeclaringType.FullName;
                methodName = method.Name;
            }

            LogManager.GetCurrentClassLogger().Error(errorMessage + " " + GetStackFrameString(frame, exception.Message));
            VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(errorCode));

            if (typeFullName == null)
            {
                return;
            }

            if (typeFullName.Contains("TM.LogicOfLocation") || string.IsNullOrEmpty(typeFullName))
            {
                errorMessageToServer = exception.Message;
            }
            else
            {
                typeFullName = typeFullName.Split('_')[0];
                errorMessageToServer = "Object " + typeFullName + " has error in method " + methodName + ". Exception = " + exception.Message;
            }

        }

        private string GetStackFrameString(StackFrame frame, string message)
        {
            int line = frame.GetFileLineNumber();
            string result = $"location Id = {_worldLocationId}; Line: {line}; Message:" + message;

            return result;
        }

        public void Clear()
        {
            Logic = null;
        }
    }
}