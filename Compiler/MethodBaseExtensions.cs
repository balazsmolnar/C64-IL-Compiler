using System.Reflection;

namespace Compiler
{
    static class MethodBaseExtension {
        static public string GetLabel(this MethodBase method){
            return $"{method.DeclaringType.Name}_{method.Name}".Replace(".", "_");
        }
    }
}