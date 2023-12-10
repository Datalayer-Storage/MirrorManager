using System.CommandLine.Invocation;
using System.CommandLine;
using System.Reflection;

static class ContextBinder
{
    public static void BindToContext(this Command command, InvocationContext context, MethodInfo method, IServiceProvider services)
    {
        var type = method.DeclaringType ?? throw new InvalidOperationException($"Could not get declaring type for method {method.Name}");
        var target = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Could not create instance of type {type.FullName}");

        // bind all the arguments to the instance
        foreach (var argument in command.Arguments)
        {
            var property = type.GetProperty(argument.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is not null)
            {
                var value = context.ParseResult.GetValueForArgument(argument);
                property.SetValue(target, value);
            }
        }

        // bind all the options to the instance
        foreach (var option in command.Options)
        {
            var property = type.GetProperty(option.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is not null)
            {
                var value = context.ParseResult.GetValueForOption(option);
                property.SetValue(target, value);
            }
        }

        var task = method.Invoke(target, services.GetAsArguments(method)) as Task<int> ?? throw new InvalidOperationException($"Could not invoke method {method.Name}");
        context.ExitCode = task.GetAwaiter().GetResult();
    }

    private static object[] GetAsArguments(this IServiceProvider services, MethodInfo method)
    {
        // for every parameter the target method has, get the service from the container with a matching type
        var parameters = method.GetParameters();
        var arguments = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            arguments[i] = services.GetService(parameter.ParameterType) ?? throw new InvalidOperationException($"Could not get service of type {parameter.ParameterType.FullName}");
        }

        return arguments;
    }
}
