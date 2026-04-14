using System.Text.RegularExpressions;

namespace E_commerce.v1.Application.Tests;

public class ArchitectureRulesTests
{
    [Fact]
    public void should_define_generic_repository_contract_and_implementation()
    {
        var root = GetSolutionRoot();
        var genericInterface = Path.Combine(root, "E-commerce.v1.Application", "Interfaces", "IGenericRepository.cs");
        var genericImplementation = Path.Combine(root, "E-commerce.v1.Infrastructure", "Repositories", "GenericRepository.cs");

        Assert.True(File.Exists(genericInterface), "IGenericRepository.cs must exist.");
        Assert.True(File.Exists(genericImplementation), "GenericRepository.cs must exist.");
    }

    [Fact]
    public void should_not_cast_iappdbcontext_to_dbcontext_in_application_handlers()
    {
        var applicationPath = Path.Combine(GetSolutionRoot(), "E-commerce.v1.Application");
        var files = Directory.GetFiles(applicationPath, "*Handler.cs", SearchOption.AllDirectories);
        var castRegex = new Regex(@"is\s+not\s+DbContext", RegexOptions.Compiled);

        var offenders = files
            .Where(f => castRegex.IsMatch(File.ReadAllText(f)))
            .ToList();

        Assert.Empty(offenders);
    }

    private static string GetSolutionRoot()
    {
        var current = AppContext.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.GetFullPath(Path.Combine(current, string.Concat(Enumerable.Repeat("..\\", i))));
            if (Directory.Exists(Path.Combine(candidate, "E-commerce.v1.Application")))
                return candidate;
        }

        throw new DirectoryNotFoundException("Không tìm thấy root của solution để chạy architecture tests.");
    }
}
