using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenerator;

[Generator]
public class GeneratorAnalyzer : IIncrementalGenerator
{
    static readonly DiagnosticDescriptor wrongEnumValue =
        new("GEN_01",
            "Unsupported Enum value",
            "The enum value {0} is not supported",
            "Generation",
            DiagnosticSeverity.Error, isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "Code.StringificationAttribute",
            predicate: static (syntaxNode, cancellationToken) => syntaxNode is PropertyDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                var containingClass = context.TargetSymbol.ContainingType;
                var attribute = context.Attributes.Single();
                var argument = attribute.ConstructorArguments.Single();
                var argumentValue = (int)argument.Value!;

                return new Model(
                    Namespace: containingClass.ContainingNamespace?.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    ClassName: containingClass.Name,
                    PropertyName: context.TargetSymbol.Name,
                    OutputType: argumentValue,
                    Location: LocationInfo.CreateFrom(context.TargetNode.GetLocation()));
            }
        );

        var groupdByClassPileline = 
            pipeline
                .Collect()
                .SelectMany((models, ct) => models.GroupBy(m => (m.Namespace, m.ClassName)));

        context.RegisterSourceOutput(groupdByClassPileline, static (context, modelGroup) =>
        {
            StringBuilder sb = new();

            var (ns, className) = modelGroup.Key;
            GeneratePreamble(ns, className);

            foreach (var model in modelGroup.OrderBy(m => m.PropertyName))
                GeneratePropertyStringifier(model);

            GeneratePostable();

            var result = sb.ToString();
            var sourceText = SourceText.From(result, Encoding.UTF8);
            context.AddSource($"{className}.g.cs", sourceText);

            void GeneratePreamble(string? ns, string className)
            {
                if (ns is not null)
                {
                    sb.AppendLine($"namespace {ns};");
                    sb.AppendLine();
                }

                sb.AppendLine($"partial class {className}");
                sb.AppendLine("{");
            }

            void GeneratePropertyStringifier(Model model)
            {
                var roundArgs = model.OutputType switch
                {
                    0 /*StringificationType.Out4*/ => "4, MidpointRounding.AwayFromZero",
                    1 /*StringificationType.Even4*/ => "4, MidpointRounding.ToEven",
                    _ => null
                };
                if (roundArgs is null)
                    context.ReportDiagnostic(
                        Diagnostic.Create(wrongEnumValue, model.Location?.ToLocation(), model.OutputType));
                else
                    sb.AppendLine(
                        $$"""
                            public string Get{{model.PropertyName}}String() =>
                                decimal.Round({{model.PropertyName}}, {{roundArgs}}).ToString();
                        """);
            }

            void GeneratePostable()
            {
                sb.AppendLine("}");
            }
        });
    }
}

public record Model(string? Namespace, string ClassName, string PropertyName, int OutputType, LocationInfo? Location);
