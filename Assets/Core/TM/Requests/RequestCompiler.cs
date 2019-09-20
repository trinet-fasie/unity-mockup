using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TM.Errors;
using TM.Data;
using TM.Experemental;
using TM.UI.VRErrorManager;

namespace TM.WWW
{
    public class RequestCompiler : Request
    {
        public RequestCompiler(string code)
        {
            UserData = new object[]{code};
            RequestManager.AddRequest(this);
            Uri = "Compiler";
        }

        protected override IEnumerator SendRequest()
        {
            yield return Get();
        }

        #region GET

        private IEnumerator Get()
        {
            Type type = null;
            string code = (string) UserData[0];
            bool done = false;
            DateTime start = DateTime.Now;
            int milliseconds = 0;
            bool errorInCode = false;
            List<CompilerError> errors = new List<CompilerError>();

            
                try
                {
                    type = RuntimeCompiler.CompileType(code, ref errorInCode, ref errors);
                    TimeSpan span = DateTime.Now - start;
                    milliseconds = span.Milliseconds;
                    done = true;
                }
                catch (Exception ex)
                {
                    done = true;
                    Debug.LogError(ex.StackTrace);
                }



            while (!done)
            {
                yield return null;
            }

            if (errorInCode)
            {
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(TM.Errors.ErrorCode.CompileCodeError));
            }

            if (type == null && errorInCode)
            {
                ((IRequest) this).OnResponseError($"Compile code error! {ToSingleString(errors)} ");
            }
            else
            {
                ResponseCompiler response = new ResponseCompiler {CompiledType = type, Milliseconds = milliseconds};
                ((IRequest) this).OnResponseDone(response);
            }

            yield return true;

        }

        #endregion

        #region METHODS
        private string ToSingleString(List<CompilerError> list)
        {
            string result = "";

            foreach (CompilerError error in list)
            {
                result += $"Line: {error.Line}; Column: {error.Column}; Message: {error.ErrorText}\n";
            }

            return result;
        }
        #endregion

    }
}