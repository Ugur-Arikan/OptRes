global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using static OptRes.Ext;

internal static class Exc
{
    internal static NotImplementedException MustNotReach = new("Must not have reached here!");
}
