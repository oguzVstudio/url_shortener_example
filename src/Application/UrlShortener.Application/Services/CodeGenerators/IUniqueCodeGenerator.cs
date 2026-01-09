namespace UrlShortener.Application.Services.CodeGenerators;

public interface IUniqueCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}