using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Common.SourceGenerators
{
    public class AutoCloneableExtensionsClassBuilder
    {
        private INamedTypeSymbol Class { get; }
        private ImmutableList<IPropertySymbol> Properties { get; }
        public string Value { get; }

        protected AutoCloneableExtensionsClassBuilder(INamedTypeSymbol classSymbol)
        {
            Class = classSymbol;
            Properties = Class.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => !x.IsStatic && x.SetMethod != null)
                .ToImmutableList();
            Value = BuildValue();
        }

        public static string Build(INamedTypeSymbol classSymbol)
        {
            return new AutoCloneableExtensionsClassBuilder(classSymbol).Value;
        }

        private string BuildValue()
        {
            var @namespace = Class.ContainingNamespace.ToDisplayString();
            var properties = BuildProperties();
            return $@"
                using System;

                namespace {@namespace}
                {{
                    public static class {Class.Name}AutoCloneableExtensions
                    {{
                        {properties}
                    }}
                }}
            ";
        }

        private string BuildProperties()
        {
            return string.Join(Environment.NewLine, Properties.Select(BuildProperty));
        }

        private string BuildProperty(IPropertySymbol property)
        {
            var arg = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
            var type = property.Type.ToDisplayString();
            const string cloneableTypeName = "Common.IAutoCloneable<T>";
            return $@"
                public static T With{property.Name}<T>(this T target, {type} {arg}) where T : {Class.Name}, {cloneableTypeName}
                {{
                    var clone = (({cloneableTypeName})target).Clone();
                    clone.{property.Name} = {arg};
                    return clone;
                }}
            ";
        }
    }
}
