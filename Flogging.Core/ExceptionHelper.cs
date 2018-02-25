using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flogging.Core
{
    public static class ExceptionHelper
    {
        public static CustomException ToCustomException(this Exception ex)
        {
            if (ex == null)
                return null;
            var error = new CustomException
            {
                Message = ex.Message,
                ExceptionName = ex.GetType().Name,
                ModuleName = ex.TargetSite?.Module.Name,
                DeclaringTypeName = ex.TargetSite?.DeclaringType?.Name,
                TargetSiteName = ex.TargetSite?.Name,
                StackTrace = ex.StackTrace,
                Data = new List<DictEntry>()
            };
            foreach (var dataKey in ex.Data.Keys)
            {
                error.Data.Add(
                    new DictEntry
                    {
                        Key = dataKey.ToString(),
                        Value = ex.Data[dataKey].ToString()
                    }
                );
            }
            if (ex.InnerException != null)
                error.InnerException = ex.InnerException.ToCustomException();
            return error;
        }
        public static string ToBetterString(this Exception ex, string prepend = null)
        {
            var exceptionMessage = new StringBuilder();
            exceptionMessage.Append("\n" + prepend + "Exception:" + ex.GetType());
            exceptionMessage.Append("\n" + prepend + "Message:" + ex.Message);
            exceptionMessage.Append(GetOtherExceptionProperties(ex, "\n" + prepend));
            exceptionMessage.Append("\n" + prepend + "Source:" + ex.Source);
            exceptionMessage.Append("\n" + prepend + "StackTrace:" + ex.StackTrace);
            exceptionMessage.Append(GetExceptionData("\n" + prepend, ex));
            if (ex.InnerException != null)
                exceptionMessage.Append(
                    "\n" + prepend + "InnerException: " 
                    + ex.InnerException.ToBetterString(prepend + "\t")
                );
            return exceptionMessage.ToString();
        }
        private static string GetExceptionData(string prependText, Exception exception)
        {
            var exData = new StringBuilder();
            foreach 
            (
                var key 
                in 
                exception.Data.Keys.Cast<object>().Where(key => exception.Data[key] != null)
            )
                exData.Append(prependText + string.Format("DATA-{0}:{1}", key, exception.Data[key]));
            return exData.ToString();
        }
        private static string GetOtherExceptionProperties(Exception exception, string s)
        {
            var allOtherProps = new StringBuilder();
            var exPropList = exception.GetType().GetProperties();
            var propertiesAlreadyHandled = new List<string>
            {
                "StackTrace", "Message", "InnerException", "Data",
                "HelpLink", "Source", "TargetSite"
            };
            foreach 
            (
                var prop 
                in 
                exPropList.Where(prop => !propertiesAlreadyHandled.Contains(prop.Name))
            )
            {
                var propObject = 
                    exception.GetType().GetProperty(prop.Name).GetValue(exception, null);
                var propEnumerable = propObject as IEnumerable;
                if (propEnumerable == null || propObject is string)
                    allOtherProps.Append(
                        s + string.Format("{0} : {1}", prop.Name, propObject)
                    );
                else
                {
                    var enumerableSb = new StringBuilder();
                    foreach (var item in propEnumerable)
                        enumerableSb.Append(item + "|");
                    allOtherProps.Append(
                        s + string.Format("{0} : {1}", prop.Name, enumerableSb)
                    );
                }
            }
            return allOtherProps.ToString();
        }
    }
}