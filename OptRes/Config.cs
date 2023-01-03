using System.Diagnostics;
using System.Text;

namespace OptRes;

/// <summary>
/// Static configurations for error messages of the Err variant of <see cref="Res"/> or <see cref="Res{T}"/> result types.
/// </summary>
public static class Config
{
    /// <summary>
    /// When true stack trace will be added to the error messages of Res types in cases of exceptions.
    /// </summary>
    public static bool AddStackTraceToErr { get; set; } = false;
    /// <summary>
    /// Method to convert (message, when, exception) into an error string.
    /// </summary>
    public static Func<(string Message, string When, Exception? Exception), string> GetErrorString { get; set; } = DefGetErrorString;


    // constants
    internal const string ErrParserFailed = "Failed parsing.";


    // helpers
    static string DefGetErrorString((string Message, string When, Exception? Exception) err)
    {
        if (err.Message.StartsWith("Err") && err.When.Length == 0 && err.Exception == null)
            return err.Message;

        if (err.When.Length == 0)
        {
            var sb = new StringBuilder();
            StackTrace trace = new();
            var frames = trace.GetFrames();
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method != null)
                {
                    var methodName = method.Name;
                    if (methodName.StartsWith('<'))
                        continue;
                    var declaringType = method.DeclaringType;
                    if (declaringType != null && declaringType.Assembly != null && declaringType.Assembly.FullName != null && declaringType.Assembly.FullName.Contains("OptRes"))
                        continue;

                    sb.Append(" > ")
                        .Append(declaringType?.FullName)
                        .Append('.')
                        .AppendLine(methodName);
                }
            }
            err.When = sb.ToString();
        }
        

        if (err.Exception == null)
        {
            if (err.Message.Length == 0)
            {
                if (err.When.Length == 0)
                    return "Err";
                else
                    return string.Format("Err [{0}]", err.When);
            }
            else
            {
                if (err.When.Length == 0)
                    return string.Format("Err\n=> {0}\n", err.Message);
                else
                    return string.Format("Err [{0}]:\n=> {1}\n", err.When, err.Message);
            }
        }
        else
        {
            if (AddStackTraceToErr)
            {
                if (err.Message.Length == 0)
                {
                    if (err.When.Length == 0)
                        return string.Format("Err\n=> {0}: {1}\n{2}\n", err.Exception.GetType().Name, err.Exception.Message, err.Exception.StackTrace);
                    else
                        return string.Format("Err [{0}]\n=> {1}: {2}\n{3}\n", err.When, err.Exception.GetType().Name, err.Exception.Message, err.Exception.StackTrace);
                }
                else
                {
                    if (err.When.Length == 0)
                        return string.Format("Err\n=> {0}: {1}\n=> {2}\n{3}\n", err.Exception.GetType().Name, err.Exception.Message, err.Message, err.Exception.StackTrace);
                    else
                        return string.Format("Err [{0}]:\n=> {1}: {2}\n=> {3}\n{4}\n", err.When, err.Exception.GetType().Name, err.Exception.Message, err.Message, err.Exception.StackTrace);
                }
            }
            else
            {
                if (err.Message.Length == 0)
                {
                    if (err.When.Length == 0)
                        return string.Format("Err\n=> {0}: {1}\n", err.Exception.GetType().Name, err.Exception.Message);
                    else
                        return string.Format("Err [{0}]\n=> {1}: {2}\n", err.When, err.Exception.GetType().Name, err.Exception.Message);
                }
                else
                {
                    if (err.When.Length == 0)
                        return string.Format("Err\n=> {0}: {1}\n=> {2}\n", err.Exception.GetType().Name, err.Exception.Message, err.Message);
                    else
                        return string.Format("Err [{0}]:\n=> {1}: {2}\n=> {3}\n", err.When, err.Exception.GetType().Name, err.Exception.Message, err.Message);
                }
            }
        }
    }
}
