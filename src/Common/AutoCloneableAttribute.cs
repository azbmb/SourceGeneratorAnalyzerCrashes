using System;

namespace Common.SourceGenerators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCloneableAttribute : Attribute
    {
    }
}
