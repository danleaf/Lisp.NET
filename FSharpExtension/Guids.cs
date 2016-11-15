// Guids.cs
// MUST match guids.h
using System;

namespace Danan.FSharpExtension
{
    static class GuidList
    {
        public const string guidFSharpExtensionPkgString = "2d1610b5-1c4c-4ca5-b14e-9278d9d511b5";
        public const string guidFSharpExtensionCmdSetString = "c6d91049-503c-4551-95ed-71b54575197f";

        public static readonly Guid guidFSharpExtensionCmdSet = new Guid(guidFSharpExtensionCmdSetString);
    };
}