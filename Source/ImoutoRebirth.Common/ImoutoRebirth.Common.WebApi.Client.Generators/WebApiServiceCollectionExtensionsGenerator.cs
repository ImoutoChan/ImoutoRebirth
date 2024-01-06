using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ImoutoRebirth.Common.WebApi.Client.Generators;

[Generator]
public class WebApiServiceCollectionExtensionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax s && s.Modifiers.Any(x => x.Text == "partial"),
            static (ctx, _) => ctx);

        var clients = provider.Select(
                (x, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    var client = x.Node;
                    var semanticModel = x.SemanticModel;
                    var clientSymbol = semanticModel.GetDeclaredSymbol(client, ct);
        
                    return (client, semanticModel, clientSymbol);
                })
            .Where(x => x.clientSymbol is ITypeSymbol)
            .Select((x, _) => (x.client, x.semanticModel, clientSymbol: (ITypeSymbol)x.clientSymbol!))
            .Where(x => x.clientSymbol.Name.EndsWith("Client"))
            .Select(
                (x, ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return (Class: x.client, ClassType: x.clientSymbol);
                });

        var compilationAndClientClasses = context.CompilationProvider.Combine(clients.Collect());

        
        context.RegisterSourceOutput(
            compilationAndClientClasses,
            (spc, c) =>
            { 
                if (c.Right.IsEmpty)
                    return;

                spc.AddSource(
                    $"ClientsDiExtensions.g.cs",
                    SourceText.From(
                        GenerateRegistration(c.Right),
                        Encoding.UTF8));
            });
    }

    private string GenerateRegistration(ImmutableArray<(SyntaxNode Class, ITypeSymbol ClassType)> webApiClientClasses)
    {
        var nameSpace = webApiClientClasses.First().ClassType.ContainingNamespace;
        var names = webApiClientClasses.Select(x => x.ClassType.Name).ToArray();

        // ImoutoRebirth.*.WebApi.Client
        var serviceName = nameSpace.ToString()!.Split('.')[1];

        return 
            $$"""
            using ImoutoRebirth.Common.WebApi.Client;
            using Microsoft.Extensions.DependencyInjection;
            
            namespace {{nameSpace}};
            
            public static class WebApiServiceCollectionExtensions
            {
                public static IServiceCollection Add{{serviceName}}WebApiClients(this IServiceCollection services, string baseUri)
                    => services
                        .{{string.Join("\r\n            .", names.Select(x => $"AddNSwagGeneratedWebApiClient<{x}>(baseUri)"))}};
            }
            """;
    }
}
