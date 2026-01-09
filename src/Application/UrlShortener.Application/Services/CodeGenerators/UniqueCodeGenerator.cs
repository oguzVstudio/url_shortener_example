using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace UrlShortener.Application.Services.CodeGenerators;

public class UniqueCodeGenerator : IUniqueCodeGenerator
{
    private readonly CodeGeneratorSettings _codeGeneratorSettings;

    public UniqueCodeGenerator(IOptions<CodeGeneratorSettings> codeGeneratorSettings)
    {
        _codeGeneratorSettings = codeGeneratorSettings.Value;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        Span<char> buffer = stackalloc char[_codeGeneratorSettings.Length];

        for (var i = 0; i < buffer.Length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(_codeGeneratorSettings.Alphabet.Length);
            buffer[i] = _codeGeneratorSettings.Alphabet[index];
        }

        var code = new string(buffer);
        return Task.FromResult(code);
    }
}