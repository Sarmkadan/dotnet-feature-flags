using System;
using System.Collections.Generic;
using System.Linq;

namespace FeatureFlags.Exceptions
{
    public static class ValidationExceptionExtensions
    {
        public static bool HasErrors(this ValidationException exception)
        {
            return exception.Errors != null && exception.Errors.Any();
        }

        public static string GetErrorMessage(this ValidationException exception, string key)
        {
            if (exception.Errors == null) return string.Empty;
            return exception.Errors.TryGetValue(key, out string value) ? value : string.Empty;
        }

        public static Dictionary<string, string> ToFlattenedErrorDictionary(this ValidationException exception)
        {
            var flattenedErrors = new Dictionary<string, string>();
            if (exception.Errors != null)
            {
                foreach (var error in exception.Errors)
                {
                    flattenedErrors.Add(error.Key, error.Value);
                }
            }
            return flattenedErrors;
        }
    }
}
