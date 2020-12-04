using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Common.SourceGenerators
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver) return;
            var types = receiver.CandidateTypes
                .Select(x => context.Compilation.GetSemanticModel(x.SyntaxTree).GetDeclaredSymbol(x)!)
                .ToImmutableList();
            AddAutoCloneableClasses(context, types);
        }

        private static void AddAutoCloneableClasses(GeneratorExecutionContext context, ImmutableList<INamedTypeSymbol> types)
        {
            foreach (var type in types)
            {
                addSource($"{type.Name}.AutoCloneable.g", AutoCloneablePartialClassBuilder.Build(type));
                //NOTE: If this addition is disabled, the crashes to the Roslyn Analyzer process do not occur.
                //      What is it about these static extensions that causes the process to crash?
                addSource($"{type.Name}.AutoCloneableExtensions.g", AutoCloneableExtensionsClassBuilder.Build(type));

                void addSource(string sourceFileName, string sourceCode)
                {
                    var sourceText = CSharpSyntaxTree
                        .ParseText(SourceText.From(sourceCode, Encoding.UTF8))
                        .GetRoot().NormalizeWhitespace()
                        .GetText(Encoding.UTF8);
                    context.AddSource(sourceFileName, sourceText);
                }
            }
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private static readonly string attributeName = nameof(AutoCloneableAttribute).Replace("Attribute", "");

            public List<TypeDeclarationSyntax> CandidateTypes { get; } = new List<TypeDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is not TypeDeclarationSyntax typeDeclarationSyntax) return;
                var hasAutoCloneableAttribute = typeDeclarationSyntax.AttributeLists
                    .SelectMany(attributeList => attributeList.Attributes, (_, attribute) => attribute.Name.ToString())
                    .Any(a => a == attributeName);
                if (hasAutoCloneableAttribute) CandidateTypes.Add(typeDeclarationSyntax);
            }
        }
    }
}
