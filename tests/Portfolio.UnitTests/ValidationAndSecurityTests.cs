using FluentAssertions;
using Portfolio.Application.Features.Contact;
using Portfolio.Application.Features.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Services;
using Xunit;

namespace Portfolio.UnitTests;

public class ValidationAndSecurityTests
{
    [Fact]
    public void CreateProjectValidator_ShouldPass_WhenCommandIsValid()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand(
            Slug: "scalable-ecommerce-platform",
            Title: "Scalable E-Commerce Platform",
            ShortSummary: "A modern distributed e-commerce system built with .NET and Angular.",
            Category: "Full-Stack Web",
            ProblemContextMarkdown: "Context...",
            GoalsMarkdown: "Goals...",
            RoleContributionsMarkdown: "Contributions...",
            ArchitectureOverviewMarkdown: "Architecture...",
            ChallengesDecisionsMarkdown: "Challenges...",
            OutcomesMarkdown: "Outcomes...",
            TechnologyTags: new List<string> { ".NET 10", "PostgreSQL", "Angular" },
            CoverImageAssetId: Guid.NewGuid(),
            LiveDemoUrl: "https://demo.example.com",
            RepositoryUrl: "https://github.com/example/repo",
            IsFeatured: true,
            DisplayOrder: 1,
            Status: ContentStatus.Published,
            SeoTitle: "E-Commerce Platform",
            SeoDescription: "Case study"
        );

        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Invalid Slug With Spaces")]
    [InlineData("invalid_slug_with_underscores")]
    [InlineData("InvalidUppercase")]
    public void CreateProjectValidator_ShouldFail_WhenSlugIsInvalid(string invalidSlug)
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand(
            Slug: invalidSlug,
            Title: "Valid Title",
            ShortSummary: "Valid Short Summary",
            Category: "Web",
            ProblemContextMarkdown: null,
            GoalsMarkdown: null,
            RoleContributionsMarkdown: null,
            ArchitectureOverviewMarkdown: null,
            ChallengesDecisionsMarkdown: null,
            OutcomesMarkdown: null,
            TechnologyTags: new List<string>(),
            CoverImageAssetId: null,
            LiveDemoUrl: null,
            RepositoryUrl: null,
            IsFeatured: false,
            DisplayOrder: 0,
            Status: ContentStatus.Draft,
            SeoTitle: null,
            SeoDescription: null
        );

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public void CreateProjectValidator_ShouldFail_WhenPublishedWithoutCoverImage()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand(
            Slug: "valid-slug",
            Title: "Valid Title",
            ShortSummary: "Valid Summary",
            Category: "Web",
            ProblemContextMarkdown: null,
            GoalsMarkdown: null,
            RoleContributionsMarkdown: null,
            ArchitectureOverviewMarkdown: null,
            ChallengesDecisionsMarkdown: null,
            OutcomesMarkdown: null,
            TechnologyTags: new List<string>(),
            CoverImageAssetId: null,
            LiveDemoUrl: null,
            RepositoryUrl: null,
            IsFeatured: false,
            DisplayOrder: 0,
            Status: ContentStatus.Published,
            SeoTitle: null,
            SeoDescription: null
        );

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CoverImageAssetId");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@missing-user.com")]
    [InlineData("missing-domain@")]
    public void SubmitContactValidator_ShouldFail_WhenEmailIsInvalid(string invalidEmail)
    {
        var validator = new SubmitContactCommandValidator();
        var command = new SubmitContactCommand("John Doe", invalidEmail, "Inquiry", "Hello world");

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void SubmitContactValidator_ShouldPass_WhenFieldsAreValid()
    {
        var validator = new SubmitContactCommandValidator();
        var command = new SubmitContactCommand("Jane Doe", "jane@example.com", "Project Inquiry", "I would like to discuss a project.");

        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyCorrectly()
    {
        var hasher = new PasswordHasher();
        var password = "SecurePassword@123";

        var hash = hasher.HashPassword(password);
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);

        hasher.VerifyPassword(password, hash).Should().BeTrue();
        hasher.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_ShouldReflectActiveStateCorrectly()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            AdminUserId = Guid.NewGuid(),
            TokenHash = "hash123",
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedAtUtc = DateTime.UtcNow
        };

        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();

        // Revoke
        token.RevokedAtUtc = DateTime.UtcNow;
        token.IsActive.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();

        // Expire
        token.RevokedAtUtc = null;
        token.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5);
        token.IsActive.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }
}

