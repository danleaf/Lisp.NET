// Guids.cs
// MUST match guids.h
using System;

namespace Dandan.VisualLisp
{
    static class GuidList
    {
        public const string guidPackageString = "328ea31e-b26d-48e4-84b0-fcc0bcebdc49";
        public const string guidCmdSetString = "ec542245-6280-4956-9bd7-362b94dad509";
        public const string guidProjectFactoryString = "ff542415-6480-7342-6434-773b94fba901";
        public const string guidLuanguageString = "65076DCA-31A8-4999-863A-5BCC7E4DF99D";

        public static readonly Guid guidVisualLispCmdSet = new Guid(guidCmdSetString);
        public static readonly Guid guidPackage = new Guid(guidPackageString);
        public static readonly Guid guidProjectFactory = new Guid(guidProjectFactoryString); 
    };
}