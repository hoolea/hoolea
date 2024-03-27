using System;
using System.Linq;
using System.Reflection;

namespace Documentation
{
    // Обобщенный класс Specifier<T>, который реализует интерфейс ISpecifier
    public class Specifier<T> : ISpecifier
    {
        // Получение описания API
        public string GetApiDescription() => typeof(T).GetCustomAttributes()
            .OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        // Получение названий методов API
        public string[] GetApiMethodNames() => typeof(T).GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<ApiMethodAttribute>().Any())
            .Select(m => m.Name)
            .ToArray();

        // Получение описания метода API по его имени
        public string GetApiMethodDescription(string methodName) =>
            typeof(T).GetMethod(methodName)?.GetCustomAttributes().OfType<ApiDescriptionAttribute>()
            .FirstOrDefault()?.Description;

        // Получение названий параметров метода API по имени метода
        public string[] GetApiMethodParamNames(string methodName) => typeof(T).GetMethod(methodName)?.GetParameters()
            .Select(parameter => parameter.Name).ToArray();

        // Получение описания параметра метода API по имени метода и имени параметра
        public string GetApiMethodParamDescription(string methodName, string paramName) =>
            typeof(T).GetMethod(methodName) == null ? null : !typeof(T).GetMethod(methodName).
            GetParameters().Where(param => param.Name == paramName).Any() ? null :
            typeof(T).GetMethod(methodName).GetParameters().Where(param => param.Name == paramName).First()
            .GetCustomAttributes().OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;

        // Получение полного описания параметра метода API (включая валидацию)
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
                        .FirstOrDefault() != null) ? parameter.First().GetCustomAttributes()
                        .OfType<ApiRequiredAttribute>().FirstOrDefault().Required : result.Required;
                }
            return result;
        }

        // Получение полного описания метода API (включая параметры и возвращаемое значение)
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
                CalculateParameter(returnParameter, returnParamDescription);
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

        // Метод для вычисления параметра
        private static void CalculateParameter(ParameterInfo returnParameter, ApiParamDescription returnParamDescription)
        {
            returnParamDescription.ParamDescription.Description = returnParameter.GetCustomAttributes()
                .OfType<ApiDescriptionAttribute>().FirstOrDefault()?.Description;
            returnParamDescription.MinValue = returnParameter.GetCustomAttributes()
                .OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MinValue;
            returnParamDescription.MaxValue = returnParameter.GetCustomAttributes()
                .OfType<ApiIntValidationAttribute>().FirstOrDefault()?.MaxValue;
        }
    }
}
