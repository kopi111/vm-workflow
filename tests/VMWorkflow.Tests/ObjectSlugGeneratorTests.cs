using VMWorkflow.Domain.Enums;
using VMWorkflow.Domain.Services;
using Xunit;

namespace VMWorkflow.Tests;

public class ObjectSlugGeneratorTests
{
    [Fact]
    public void Generate_BasicInput_ReturnsExpectedSlug()
    {
        var slug = ObjectSlugGenerator.Generate("Payroll", EnvironmentType.Production, 1);
        Assert.Equal("payroll-prod-01", slug);
    }

    [Fact]
    public void Generate_WithSpaces_NormalizesSlug()
    {
        var slug = ObjectSlugGenerator.Generate("My App Name", EnvironmentType.Development, 3);
        Assert.Equal("my-app-name-dev-03", slug);
    }

    [Fact]
    public void Generate_WithSpecialChars_StripsInvalidChars()
    {
        var slug = ObjectSlugGenerator.Generate("App@#$Test!", EnvironmentType.Staging, 5);
        Assert.Equal("apptest-staging-05", slug);
    }

    [Fact]
    public void Generate_WithUnderscores_ConvertsToDashes()
    {
        var slug = ObjectSlugGenerator.Generate("my_app_name", EnvironmentType.Production, 1);
        Assert.Equal("my-app-name-prod-01", slug);
    }

    [Theory]
    [InlineData(EnvironmentType.Production, "prod")]
    [InlineData(EnvironmentType.Staging, "staging")]
    [InlineData(EnvironmentType.Development, "dev")]
    [InlineData(EnvironmentType.DisasterRecovery, "dr")]
    public void Generate_AllEnvironments_MapsCorrectly(EnvironmentType env, string expected)
    {
        var slug = ObjectSlugGenerator.Generate("test", env, 1);
        Assert.Equal($"test-{expected}-01", slug);
    }

    [Fact]
    public void Generate_SequenceNumber_PadsToTwoDigits()
    {
        var slug = ObjectSlugGenerator.Generate("app", EnvironmentType.Production, 9);
        Assert.Equal("app-prod-09", slug);

        var slug2 = ObjectSlugGenerator.Generate("app", EnvironmentType.Production, 12);
        Assert.Equal("app-prod-12", slug2);
    }
}
