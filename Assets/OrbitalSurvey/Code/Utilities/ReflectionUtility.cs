using System;
using System.Reflection;

namespace OrbitalSurvey.Utilities
{
    internal static class ReflectionUtility
    {
        internal static T GetPrivateField<T>(object obj, string fieldName)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null)
                    return (T)field.GetValue(obj);
                type = type.BaseType;
            }
            throw new MissingFieldException(obj.GetType().FullName, fieldName);
        }

        internal static void InvokePrivateMethod(object obj, string methodName, params object[] args)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    method.Invoke(obj, args);
                    return;
                }
                type = type.BaseType;
            }
            throw new MissingMethodException(obj.GetType().FullName, methodName);
        }
    }
}
