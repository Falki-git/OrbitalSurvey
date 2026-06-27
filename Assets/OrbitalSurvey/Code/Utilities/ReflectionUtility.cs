using System.Reflection;

namespace OrbitalSurvey.Utilities
{
    internal static class ReflectionUtility
    {
        internal static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)field.GetValue(obj);
        }

        internal static void InvokePrivateMethod(object obj, string methodName, params object[] args)
        {
            var method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, args);
        }
    }
}
