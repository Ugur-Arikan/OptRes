global using System.Runtime.CompilerServices;
global using static OptRes.Extensions;

namespace OptRes
{
    /// <summary>
    /// Library for option and lightweight-result types.
    /// <list type="bullet">
    /// <item>Opt&lt;T>: Some(T) or None.</item>
    /// <item>Res: Ok or Err(message).</item>
    /// <item>Res&lt;T>: Ok(T) or Err(message)</item>
    /// </list>
    /// In order to enable the types glboally, add the following in project's global usings file:
    /// <code>
    /// global using OptRes;
    /// global using static OptRes.Extensions;
    /// </code>
    /// 
    /// Alternatively, the scope can be limited to a file by adding the following in the particular file:
    /// <code>
    /// using OptRes;
    /// using static OptRes.Extensions;
    /// </code>
    /// </summary>
    [CompilerGeneratedAttribute()]
    class NamespaceDoc { }

    internal static class Exc
    {
        internal static NotImplementedException MustNotReach = new("Must not have reached here!");
    }
}