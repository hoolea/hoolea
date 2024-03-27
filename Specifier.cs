using System;
using System.Linq;
using System.Reflection;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        // Get the API description for the specified type T
        public string GetApiDescription() => typeof(T).GetCustomAttributes()
            .OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        // Get an array of method names that have ApiMethodAttribute for the specified type T
        public string[] GetApiMethodNames() => typeof(T).GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<ApiMethodAttribute>().Any())
            .Select(m => m.Name)
            .ToArray();

        // Get API method description for a specific method name
        public string GetApiMethodDescription(string methodName) =>
            typeof(T).GetMethod(methodName)?.GetCustomAttributes().OfType<ApiDescriptionAttribute>()
            .FirstOrDefault()?.Description;

        // Get parameter names for a specific method
        public string[] GetApiMethodParamNames(string methodName) => typeof(T).GetMethod(methodName)?.GetParameters()
            .Select(parameter => parameter.Name).ToArray();

        // Get description for a specific parameter of a method
        public string GetApiMethodParamDescription(string methodName, string paramName) =>
            typeof(T).GetMethod(methodName) == null ? null : !typeof(T).GetMethod(methodName).
            GetParameters().Where(param => param.Name == paramName).Any() ? null :
            typeof(T).GetMethod(methodName).GetParameters().Where(param => param.Name == paramName).First()
            .GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        // Get full parameter description for a specific method and parameter
        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {
            var result = new ApiParamDescription { ParamDescription = new CommonDescription(paramName) };
            var parameter = typeof(T).GetMethod(methodName)?.GetParameters().Where(param => param.Name == paramName);
            if (parameter != null && parameter.Any())
            {
                result.ParamDescription.Description = parameter.First().GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

                result.MinValue = parameter.First().GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MinValue;
                result.MaxValue = parameter.First().GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MaxValue;

                result.Required = (parameter.First().GetCustomAttributes().OfType<ApiRequiredAttribute>()
                        .FirstOrDefault() != null) ? parameter.First().GetCustomAttributes()
                            .OfType<ApiRequiredAttribute>().FirstOrDefault().Required : result.Required;
            }
            return result;
        }

        // Get full method description including parameters for a specific method
        public ApiMethodDescription GetApiMethodFullDescription(string methodName)
        {
            if (typeof(T).GetMethod(methodName).GetCustomAttributes().OfType<ApiMethodAttribute>().Any())
            {
                var result = new ApiMethodDescription
                {
                    MethodDescription = new CommonDescription(methodName, GetApiMethodDescription(methodName)),
                    ParamDescriptions = GetApiMethodParamNames(methodName)
                        .Select(param => GetApiMethodParamFullDescription(methodName, param)).ToArray()
                };

                var returnParameter = typeof(T).GetMethod(methodName).ReturnParameter;
                var returnParamDescription = new ApiParamDescription { ParamDescription = new CommonDescription() };
                CalculateParametr(returnParameter, returnParamDescription);

                if (returnParameter.GetCustomAttributes().OfType<ApiRequiredAttribute>().FirstOrDefault() != null)
                {
                    returnParamDescription.Required = returnParameter.GetCustomAttributes()
                        .OfType<ApiRequiredAttribute>().FirstOrDefault().Required;
                    result.ReturnDescription = returnParamDescription;
                }
                return result;
            }
            return null;
        }

        // Calculate parameter details for the return parameter
        private static void CalculateParametr(ParameterInfo returnParameter, ApiParamDescription returnParamDiscription)
        {
            returnParamDiscription.ParamDescription.Description = returnParameter.GetCustomAttributes()
                .OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
            returnParamDiscription.MinValue = returnParameter.GetCustomAttributes()
                .OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MinValue;
            returnParamDiscription.MaxValue = returnParameter.GetCustomAttributes()
                .OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MaxValue;
        }
    }
}
