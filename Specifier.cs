using System;
using System.Linq;
using System.Reflection;

namespace Documentation
{
    public class Specifier<T> : ISpecifier
    {
        public string GetApiDescription() => typeof(T).GetCustomAttributes()
                .OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        public string[] GetApiMethodNames() => typeof(T).GetMethods()
                .Where(m => m.GetCustomAttributes().OfType<ApiMethodAttribute>().Any())
                .Select(m => m.Name)
                .ToArray();

        public string GetApiMethodDescription(string methodName) =>
            typeof(T).GetMethod(methodName)?.GetCustomAttributes().OfType<ApiDescriptionAttribute>()
            .FirstOrDefault()?.Description;

        public string[] GetApiMethodParamNames(string methodName) => typeof(T).GetMethod(methodName)?.GetParameters()
                .Select(parameter => parameter.Name).ToArray();

        public string GetApiMethodParamDescription(string methodName, string paramName) =>
            typeof(T).GetMethod(methodName) == null ? null : !typeof(T).GetMethod(methodName).
            GetParameters().Where(param => param.Name == paramName).Any() ? null :
            typeof(T).GetMethod(methodName).GetParameters().Where(param => param.Name == paramName).First()
            .GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        public ApiParamDescription GetApiMethodParamFullDescription(string methodName, string paramName)
        {
            var result = new ApiParamDescription { ParamDescription = new CommonDescription(paramName) };
            var parameter = typeof(T).GetMethod(methodName)?.GetParameters().Where(param => param.Name == paramName);
            if (parameter != null)
                if (parameter.Any())
                {
                    result.ParamDescription.Description = parameter
                        .First().GetCustomAttributes().OfType<ApiDescriptionAttribute>()
                        .FirstOrDefault()?.Description;

                    result.MinValue = parameter
                     .First().GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MinValue;

                    result.MaxValue = parameter
                    .First().GetCustomAttributes().OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MaxValue;

                    result.Required = (parameter.First().GetCustomAttributes().OfType<ApiRequiredAttribute>()
                        .FirstOrDefault() != null) ? parameter.First().GetCustomAttributes().
                        OfType<ApiRequiredAttribute>().FirstOrDefault().Required : result.Required;
                }
            return result;
        }

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
                var returnParamDiscription = new ApiParamDescription { ParamDescription = new CommonDescription() };
                CalculateParametr(returnParameter, returnParamDiscription);
                if (returnParameter.GetCustomAttributes().OfType<ApiRequiredAttribute>().FirstOrDefault() != null)
                {
                    returnParamDiscription.Required = returnParameter.GetCustomAttributes()
                        .OfType<ApiRequiredAttribute>().FirstOrDefault().Required;
                    result.ReturnDescription = returnParamDiscription;
                }
                return result;
            }
            return null;
        }

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
