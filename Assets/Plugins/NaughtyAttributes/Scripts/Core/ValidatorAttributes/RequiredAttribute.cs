// using System;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// namespace NaughtyAttributes
// {
//     [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
//     public class RequiredAttribute : ValidatorAttribute
//     {
//         public string Message { get; private set; }
//
//         public RequiredAttribute(string message = null) 
//         {
//             Message = message;
//         }
//     }
// }

// TODO: добавить в гитхаб патч
using System;

namespace NaughtyAttributes
{
    public enum RequiredLogLevel { None, Warning, Error }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : ValidatorAttribute
    {
        public string Message { get; }
        public RequiredLogLevel LogLevel { get; }

        /// <param name="message">Кастомный текст в инспекторе</param>
        /// <param name="logLevel">Логнуть в консоль? (None/Warning/Error)</param>
        public RequiredAttribute(string message = null, RequiredLogLevel logLevel = RequiredLogLevel.Error)
        {
            Message = message;
            LogLevel = logLevel;
        }
    }
}
