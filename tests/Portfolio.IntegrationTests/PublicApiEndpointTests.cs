using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Portfolio.Application.Contracts;
using Portfolio.Application.Features.Auth;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Xunit;

namespace Portfolio.IntegrationTests;

public class PublicApiEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PublicApiEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Register a mock Unit of Work so tests don't require a live PostgreSQL instance
                var mockUow = new Mock<IApplicationUnitOfWork>();

                // Mock Profile
                mockUow.Setup(u => u.Profiles.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new Profile
                       {
                           Id = Guid.NewGuid(),
                           DisplayName = "Test Developer",
                           Headline = "Senior Software Engineer",
                           ShortIntro = "Building modern web apps",
                           BiographyMarkdown = "Experienced full-stack engineer",
                           ShowEmailPublicly = false,
                           PublicEmail = "hidden@example.com"
                       });

                // Mock Projects
                mockUow.Setup(u => u.Projects.GetPublishedProjectsAsync(It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Project>
                       {
                           new()
                           {
                               Id = Guid.NewGuid(),
                               Slug = "project-alpha",
                               Title = "Project Alpha",
                               ShortSummary = "First project summary",
                               Category = "Web",
                               Status = ContentStatus.Published
                           }
                       });

                // Mock Experiences
                mockUow.Setup(u => u.Experiences.GetPublishedOrderedAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Experience>
                       {
                           new()
                           {
                               Id = Guid.NewGuid(),
                               Organization = "Tech Corp",
                               RoleTitle = "Tech Lead",
                               Summary = "Led development teams",
                               StartDate = new DateOnly(2022, 1, 1),
                               Status = ContentStatus.Published
                           }
                       });

                // Mock Skills
                mockUow.Setup(u => u.SkillCategories.GetVisibleWithItemsAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<SkillCategory>
                       {
                           new()
                           {
                               Id = Guid.NewGuid(),
                               Name = "Languages",
                               Slug = "languages",
                               IsVisible = true,
                               Items = new List<SkillItem>
                               {
                                   new() { Id = Guid.NewGuid(), Name = "C#", ContextDescription = "Primary backend language", IsVisible = true }
                               }
                           }
                       });

                // Mock Educations
                mockUow.Setup(u => u.Educations.GetPublishedOrderedAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Education>());

                // Mock Publications
                mockUow.Setup(u => u.Publications.GetPublishedOrderedAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Publication>());

                // Mock Certifications
                mockUow.Setup(u => u.Certifications.GetPublishedOrderedAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Certification>());

                // Mock Blog
                mockUow.Setup(u => u.BlogArticles.GetPublishedArticlesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<BlogArticle>());

                services.AddScoped(_ => mockUow.Object);
            });
        });
    }

    [Fact]
    public async Task GetProfile_ReturnsOk_AndNeverLeaksHiddenEmail()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Test Developer");
        json.Should().NotContain("hidden@example.com"); // Privacy rule: email omitted when showEmailPublicly is false
    }

    [Fact]
    public async Task GetProjects_ReturnsOk_WithPublishedProjects()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("project-alpha");
    }

    [Fact]
    public async Task GetExperiences_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/experiences");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Tech Corp");
    }

    [Fact]
    public async Task GetSkills_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/skills");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("Languages");
    }

    [Fact]
    public async Task GetSitemap_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/seo/sitemap");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("/projects/project-alpha");
    }

    [Fact]
    public async Task AdminEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/admin/dashboard/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPayload_ReturnsValidationProblemDetails()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("", ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("validation errors");
    }
}

