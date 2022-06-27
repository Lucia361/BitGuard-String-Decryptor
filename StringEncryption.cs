using System;
using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System.Text;
using System.Collections.Generic;

namespace BitGuardFree
{
    internal class StringEncryption
    {
        public static void Run(ModuleDefMD module) 
        {
            int fixedStrings = 0;
            
            
            foreach (TypeDef type in module.Types)
            {
                List<MethodDef> methodsToDelete = new List<MethodDef>();
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;
                   
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].OpCode == OpCodes.Call)
                        {
                            try
                            {
                                MethodDef methodDef = (MethodDef)method.Body.Instructions[i].Operand;
                                for (int instr = 0; instr < methodDef.Body.Instructions.Count; instr++)
                                {
                                    if (methodDef.Body.Instructions[instr].OpCode == OpCodes.Ldstr && methodDef.Body.Instructions[instr + 1].OpCode == OpCodes.Ldc_I4)
                                    {
                                        methodsToDelete.Add(methodDef);
                                        int key = methodDef.Body.Instructions[instr + 1].GetLdcI4Value();
                                        string str = methodDef.Body.Instructions[instr].Operand.ToString();
                                        method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                        method.Body.Instructions.Insert(i + 1, OpCodes.Ldstr.ToInstruction(Decrypt(str, key)));
                                        fixedStrings++;
                                        break;
                                    }

                                }
                            }
                            catch { }
                        }
                        
                    }
                }
                foreach(var methods in methodsToDelete)
                    type.Methods.Remove(methods);
            }
            Console.WriteLine($"Decrypted {fixedStrings} strings");
        }
        static string Decrypt(string A_0, int A_1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < A_0.Length; i++)
            {
                stringBuilder.Append((char)((ulong)A_0[i] ^ (ulong)((long)(i % A_1))));
            }
            return stringBuilder.ToString();
        }
    }
}
