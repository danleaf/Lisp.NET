// Guids.cs
// MUST match guids.h
using System;

namespace Company.自定义项目
{
    static class GuidList
    {
        public const string guidPkgString = "48adcff8-e3e7-4214-a48f-0c3aebced03c";
        public const string guidCmdSetString = "0b7499a9-8ca7-4b5f-890c-154df4894ed1";
        public const string guidProjectFactory = "0b7499a9-8ca7-4b5f-890c-154df4894ed2";

        public static readonly Guid guidCmdSet = new Guid(guidCmdSetString);
        public static readonly Guid guidProjectFactorySet = new Guid(guidProjectFactory);
    };
}