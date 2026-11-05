using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Features.Blog;
using Portfolio.Application.Features.Certifications;
using Portfolio.Application.Features.Contact;
using Portfolio.Application.Features.Educations;
using Portfolio.Application.Features.Experiences;
using Portfolio.Application.Features.Profile;
using Portfolio.Application.Features.Projects;
using Portfolio.Application.Features.Publications;
using Portfolio.Application.Features.Skills;

namespace Portfolio.Api.Controllers.Public;

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/profile")]
public class ProfileController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["profile"])]
    [ProducesResponseType(typeof(PublicProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await SendQueryAsync(new GetPublicProfileQuery());
        return profile == null ? NotFound() : Ok(profile);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/projects")]
public class ProjectsController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["projects"])]
    [ProducesResponseType(typeof(IList<PublicProjectSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects([FromQuery] bool? featured, [FromQuery] string? category, [FromQuery] string? tag)
    {
        var projects = await SendQueryAsync(new GetPublicProjectsQuery(featured, category, tag));
        return Ok(projects);
    }

    [HttpGet("{slug}")]
    [OutputCache(Tags = ["projects"])]
    [ProducesResponseType(typeof(PublicProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var project = await SendQueryAsync(new GetPublicProjectBySlugQuery(slug));
        return project == null ? NotFound() : Ok(project);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/experiences")]
public class ExperiencesController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["experiences"])]
    [ProducesResponseType(typeof(IList<PublicExperienceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExperiences()
    {
        var list = await SendQueryAsync(new GetPublicExperiencesQuery());
        return Ok(list);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/skills")]
public class SkillsController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["skills"])]
    [ProducesResponseType(typeof(IList<PublicSkillCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills()
    {
        var list = await SendQueryAsync(new GetPublicSkillsQuery());
        return Ok(list);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/educations")]
public class EducationsController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["educations"])]
    [ProducesResponseType(typeof(IList<PublicEducationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEducations()
    {
        var list = await SendQueryAsync(new GetPublicEducationsQuery());
        return Ok(list);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/publications")]
public class PublicationsController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["publications"])]
    [ProducesResponseType(typeof(IList<PublicPublicationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublications()
    {
        var list = await SendQueryAsync(new GetPublicPublicationsQuery());
        return Ok(list);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/certifications")]
public class CertificationsController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["certifications"])]
    [ProducesResponseType(typeof(IList<PublicCertificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertifications()
    {
        var list = await SendQueryAsync(new GetPublicCertificationsQuery());
        return Ok(list);
    }
}

[AllowAnonymous]
[EnableRateLimiting("PublicReadPolicy")]
[Route("api/v1/blog")]
public class BlogController : BaseApiController
{
    [HttpGet]
    [OutputCache(Tags = ["blog"])]
    [ProducesResponseType(typeof(IList<PublicBlogSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlogPosts([FromQuery] string? tag)
    {
        var list = await SendQueryAsync(new GetPublicBlogPostsQuery(tag));
        return Ok(list);
    }

    [HttpGet("{slug}")]
    [OutputCache(Tags = ["blog"])]
    [ProducesResponseType(typeof(PublicBlogDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogPost(string slug)
    {
        var post = await SendQueryAsync(new GetPublicBlogPostBySlugQuery(slug));
        return post == null ? NotFound() : Ok(post);
    }
}

[AllowAnonymous]
[Route("api/v1/contact")]
public class ContactController : BaseApiController
{
    [HttpPost]
    [EnableRateLimiting("ContactPolicy")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitContact([FromBody] SubmitContactCommand command)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var cmd = command with
        {
            IpHash = string.IsNullOrEmpty(ip) ? null : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(ip))),
            UserAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent.Length > 300 ? userAgent[..300] : userAgent
        };

        await SendCommandAsync(cmd);
        return Accepted(new { message = "Your message has been received. Thank you!" });
    }
}

[AllowAnonymous]
[Route("api/v1/seo")]
public class SeoController : BaseApiController
{
    [HttpGet("sitemap")]
    [OutputCache(Duration = 300)]
    public async Task<IActionResult> GetSitemap()
    {
        var projects = await SendQueryAsync(new GetPublicProjectsQuery(null, null, null));
        var blogs = await SendQueryAsync(new GetPublicBlogPostsQuery(null));

        var entries = new List<object>
        {
            new { route = "/", lastModified = DateTime.UtcNow },
            new { route = "/about", lastModified = DateTime.UtcNow },
            new { route = "/projects", lastModified = DateTime.UtcNow },
            new { route = "/experiences", lastModified = DateTime.UtcNow },
            new { route = "/skills", lastModified = DateTime.UtcNow },
            new { route = "/contact", lastModified = DateTime.UtcNow }
        };

        entries.AddRange(projects.Select(p => new { route = $"/projects/{p.Slug}", lastModified = p.PublishedAtUtc ?? DateTime.UtcNow }));
        entries.AddRange(blogs.Select(b => new { route = $"/blog/{b.Slug}", lastModified = b.PublishedAtUtc ?? DateTime.UtcNow }));

        return Ok(entries);
    }
}
