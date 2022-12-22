using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Porbeagle.SourceGenerators;

internal static class BaseTypeSyntaxExtensions
{
    internal static bool IsModuleClass(
        this BaseTypeSyntax baseType,
        SemanticModel model,
        INamedTypeSymbol moduleType
    )
    {
        var typeInfo = model.GetTypeInfo(baseType.Type);

        return SymbolEqualityComparer.Default.Equals(typeInfo.Type, moduleType);
    }
}