using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Common.SourceGenerators
{
    public class AutoCloneablePartialClassBuilder
    {
        private INamedTypeSymbol Class { get; }
        private ImmutableList<IPropertySymbol> Properties { get; }
        public string Value { get; }

        protected AutoCloneablePartialClassBuilder(INamedTypeSymbol classSymbol)
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
            return new AutoCloneablePartialClassBuilder(classSymbol).Value;
        }

        private string BuildValue()
        {
            var @namespace = Class.ContainingNamespace.ToDisplayString();
            const string cloneableTypeName = "Common.IAutoCloneable";
            var cloneConstructor = BuildCloneConstructor();

            return $@"
                using System;

                namespace {@namespace} {{
                    public partial class {Class.Name} : {cloneableTypeName}<{Class.Name}> {{
                        {cloneConstructor}

                        public {Class.Name} Clone() => new {Class.Name}(this);
                    }}
                }}
            ";
        }

        public string BuildCloneConstructor()
        {
            var propertyAssignments = BuildConstructorPropertyAssignments();
            return $@"
                public {Class.Name}({Class.Name} other)
                {{
                    {propertyAssignments}
                }}
            ";
        }

        private string BuildConstructorPropertyAssignments()
        {
            return string.Join(Environment.NewLine, Properties
                .Select(property => $"{property.Name} = other.{property.Name};"));
        }
    }
}
