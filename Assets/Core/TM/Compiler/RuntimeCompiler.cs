using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modified.Mono.CSharp;
using UnityEngine;

namespace TM.Experemental
{
    public static class RuntimeCompiler
    {

        private static readonly Dictionary<string, string> AssembliesDic = new Dictionary<string, string>();
        private static string[] _assemblyList;

        public static void Init()
        {
            foreach (var assembli in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string dll = Path.GetFileName(assembli.Location);

                    if (!AssembliesDic.ContainsKey(dll))
                    {
                        AssembliesDic.Add(dll, assembli.Location);
                    }
                }
                catch (Exception)
                {
                    //
                }

            }

            _assemblyList = File.ReadAllLines(Application.dataPath + "/StreamingAssets/assemblies.txt");
        }


        public static Type CompileType(string code, ref bool errorInCode, ref List<CompilerError> errors)
        {
            var cSharpCodeCompiler = new CSharpCodeCompiler();

            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            foreach (var assembli in AssembliesDic)
            {
                foreach (var loadDll in _assemblyList)
                {
                    if (assembli.Key != loadDll)
                    {
                        continue;
                    }

                    try
                    {

                        if (!parameters.ReferencedAssemblies.Contains(assembli.Value))
                        {
                            parameters.ReferencedAssemblies.Add(assembli.Value);
                        }
                    }
                    catch
                    {
                        //
                    }
                }

            }

            foreach (string path in GameStateData.GetAssembliesPathes())
            {
                try
                {
                    if (!parameters.ReferencedAssemblies.Contains(path))
                    {
                        parameters.ReferencedAssemblies.Add(path);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var result = cSharpCodeCompiler.CompileAssemblyFromSource(parameters, code);
            var assembly = result.CompiledAssembly;

            if (!result.Errors.HasErrors)
            {
                return assembly.ExportedTypes.FirstOrDefault();
            }

            foreach (CompilerError error in result.Errors)
            {
                errorInCode = true;
                errors.Add(error); 
            }

            return null;

        }
    }
}
