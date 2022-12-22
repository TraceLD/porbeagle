using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Porbeagle.SourceGenerators;

[Generator]
public class MessageViewGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (SyntaxReceiver)context.SyntaxContextReceiver!;

        foreach (var v in receiver.MessageViews)
        {
            var generator = new MessageViewPartialFileGenerator(v, context);
            var (fileName, fileContent) = generator.GenerateFile();
            
            context.AddSource(fileName, fileContent);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class SyntaxReceiver : ISyntaxContextReceiver
    {
        private INamedTypeSymbol? _messageViewInterfaceSymbol;

        public List<RecordDeclarationSyntax> MessageViews { get; }

        public SyntaxReceiver()
            => MessageViews = new();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            _messageViewInterfaceSymbol ??=
                context.SemanticModel.Compilation.GetTypeByMetadataName("Porbeagle.IMessageView");

            if (context.Node is RecordDeclarationSyntax { BaseList: { } baseList } rds
                && baseList
                    .Types
                    .Any(t => t.IsModuleClass(context.SemanticModel, _messageViewInterfaceSymbol!))
                && HasDiscordViewAttribute(rds)
            )
            {
                MessageViews.Add(rds);
            }
        }

        private static bool HasDiscordViewAttribute(RecordDeclarationSyntax rds)
            => rds
                .AttributeLists
                .Any(x => x.Attributes.Any(attr => attr.Name.ToString().Equals("DiscordView")));
    }

    private class MessageViewPartialFileGenerator
    {
        private readonly RecordDeclarationSyntax _messageView;
        private readonly GeneratorExecutionContext _context;

        internal MessageViewPartialFileGenerator(RecordDeclarationSyntax messageView, GeneratorExecutionContext context)
        {
            _messageView = messageView;
            _context = context;
        }

        internal (string FileName, string FileContent) GenerateFile()
        {
            var components = new Dictionary<int, List<string>>(); 
            var embeds = new List<string>();
            var stickers = new List<string>();
            
            var stickerAttr = _context.Compilation.GetTypeByMetadataName("Porbeagle.Attributes.StickerAttribute");
            var actionRowAttr = _context.Compilation.GetTypeByMetadataName("Porbeagle.Attributes.ActionRowAttribute");
            var snowflakeType = _context.Compilation.GetTypeByMetadataName("Remora.Rest.Core.Snowflake");
            var embedType = _context.Compilation.GetTypeByMetadataName("Remora.Discord.API.Objects.Embed");
            

            var messageViewSymbol = (INamedTypeSymbol)_context.Compilation
                .GetSemanticModel(_messageView.SyntaxTree)
                .GetDeclaredSymbol(_messageView)!;
            var viewProperties = messageViewSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToList();
            var @namespace = messageViewSymbol.ContainingNamespace.ToDisplayString();

            foreach (var property in viewProperties)
            {
                // sticker props;
                if (property.Type.Equals(snowflakeType, SymbolEqualityComparer.Default)
                    && property
                        .GetAttributes()
                        .Any(x => x.AttributeClass?.Equals(stickerAttr, SymbolEqualityComparer.Default) ?? false)
                )
                {
                    stickers.Add(property.Name);
                }
                // embed props;
                else if (property.Type.Equals(embedType, SymbolEqualityComparer.Default))
                {
                    embeds.Add(property.Name);
                }
                // component props;
                else
                {
                    var actionRowAttrDecl = property
                        .GetAttributes()
                        .FirstOrDefault(x =>
                            x.AttributeClass?.Equals(actionRowAttr, SymbolEqualityComparer.Default) ?? false);

                    if (actionRowAttrDecl is null)
                        continue;

                    var index = (int)actionRowAttrDecl.ConstructorArguments.FirstOrDefault().Value!;

                    if (components.ContainsKey(index))
                        components[index].Add(property.Name);
                    else
                        components.Add(index, new List<string> { property.Name });
                }
            }

            var viewName = _messageView.Identifier.ToString();
            var fileName = $"{@namespace.Replace(".", "_")}_{viewName}.g.cs";

            var content = $$"""
            // auto-generated;
            namespace {{@namespace}};
            
            public partial record {{viewName}}
            {
                // Generated stickers list;
                public global::Remora.Rest.Core.Optional<global::System.Collections.Generic.IReadOnlyList<global::Remora.Rest.Core.Snowflake>> Stickers 
                    => {{GetStickersProp(stickers)}};
                    
                // Generated embeds list;
                public global::Remora.Rest.Core.Optional<global::System.Collections.Generic.IReadOnlyList<global::Remora.Discord.API.Abstractions.Objects.IEmbed>> Embeds 
                    => {{GetEmbedsProp(embeds)}};
                    
                // Generated action rows;
                public global::Remora.Rest.Core.Optional<global::System.Collections.Generic.IReadOnlyList<global::Remora.Discord.API.Abstractions.Objects.IMessageComponent>> Components 
                    => {{GenerateActionRowsProp(components)}};    
            }
            """;
            
            return (fileName, content);
        }

        private static string GetStickersProp(List<string> stickers)
        {
            if (!stickers.Any())
                return "new()";

            var stickersListElements = string.Join(", ", stickers);

            return $$"""
                    new global::System.Collections.Generic.List<global::Remora.Rest.Core.Snowflake>
                            {
                                {{stickersListElements}} 
                            }
                    """;
        }

        private static string GetEmbedsProp(List<string> embeds)
        {
            if (!embeds.Any())
                return "new()";

            var embedListElements = string.Join(", ", embeds);

            return $$"""
                    new global::System.Collections.Generic.List<global::Remora.Discord.API.Abstractions.Objects.IEmbed>
                            {
                                {{embedListElements}} 
                            }
                    """;
        }

        private static string GenerateActionRowsProp(Dictionary<int, List<string>> components)
        {
            if (!components.Any())
                return "new()";
            
            var actionRows = components
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();
            
            var actionRowsProp = $$"""
            new global::System.Collections.Generic.List<global::Remora.Discord.API.Abstractions.Objects.IActionRowComponent>
                    {
            {{string.Join(",\n", actionRows.Select(GenerateActionRow))}}
                    }
            """;

            return actionRowsProp;
        }

        private static string GenerateActionRow(List<string> actionRowComponents)
        {
            var list = $$"""
                            new global::System.Collections.Generic.List<global::Remora.Discord.API.Abstractions.Objects.IMessageComponent>
                            {
                                {{ string.Join(", ", actionRowComponents) }}  
                            }
            """;

            return $$"""
                       new global::Remora.Discord.API.Objects.ActionRowComponent
                       (
            {{list}}
                       )
            """;
        }
    }
}