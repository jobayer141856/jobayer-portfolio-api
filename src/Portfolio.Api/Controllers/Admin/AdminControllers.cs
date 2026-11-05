using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Contracts;
using Portfolio.Application.Features.Blog;
using Portfolio.Application.Features.Certifications;
using Portfolio.Application.Features.Contact;
using Portfolio.Application.Features.Educations;
using Portfolio.Application.Features.Experiences;
using Portfolio.Application.Features.Media;
using Portfolio.Application.Features.Profile;
using Portfolio.Application.Features.Projects;
using Portfolio.Application.Features.Publications;
using Portfolio.Application.Features.Skills;
using Portfolio.Domain.Enums;

namespace Portfolio.Api.Controllers.Admin;

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/dashboard")]
public class AdminDashboardController : BaseApiController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromServices] IApplicationUnitOfWork uow)
    {
        var unreadMessages = await uow.ContactMessages.GetCountAsync(m => m.Status == MessageStatus.Unread);
        var totalProjects = await uow.Projects.GetCountAsync();
        var publishedProjects = await uow.Projects.GetCountAsync(p => p.Status == ContentStatus.Published);
        var totalExperiences = await uow.Experiences.GetCountAsync();
        var totalSkills = await uow.SkillItems.GetCountAsync();
        var totalMedia = await uow.MediaAssets.GetCountAsync();

        return Ok(new
        {
            unreadMessages,
            totalProjects,
            publishedProjects,
            totalExperiences,
            totalSkills,
            totalMedia
        });
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/profile")]
public class AdminProfileController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminProfileController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await SendQueryAsync(new GetAdminProfileQuery());
        return profile == null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("profile", default);
        return Ok(updated);
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/projects")]
public class AdminProjectsController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminProjectsController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] ContentStatus? status = null)
    {
        var result = await SendQueryAsync(new GetAdminProjectsQuery(page, pageSize, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var project = await SendQueryAsync(new GetAdminProjectByIdQuery(id));
        return project == null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("projects", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("projects", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteProjectCommand(id));
        await _cacheStore.EvictByTagAsync("projects", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangeProjectStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("projects", default);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderProjectsCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("projects", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/experiences")]
public class AdminExperiencesController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminExperiencesController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await SendQueryAsync(new GetAdminExperiencesQuery());
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminExperienceByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExperienceCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("experiences", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExperienceCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("experiences", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteExperienceCommand(id));
        await _cacheStore.EvictByTagAsync("experiences", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangeExperienceStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("experiences", default);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderExperiencesCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("experiences", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/skills")]
public class AdminSkillsController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminSkillsController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await SendQueryAsync(new GetAdminSkillCategoriesQuery());
        return Ok(list);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateSkillCategoryCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("skills", default);
        return Ok(created);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateSkillCategoryCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("skills", default);
        return Ok(updated);
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await SendCommandAsync(new DeleteSkillCategoryCommand(id));
        await _cacheStore.EvictByTagAsync("skills", default);
        return NoContent();
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateSkillItemCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("skills", default);
        return Ok(created);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateSkillItemCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("skills", default);
        return Ok(updated);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        await SendCommandAsync(new DeleteSkillItemCommand(id));
        await _cacheStore.EvictByTagAsync("skills", default);
        return NoContent();
    }

    [HttpPut("reorder-categories")]
    public async Task<IActionResult> ReorderCategories([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderSkillCategoriesCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("skills", default);
        return NoContent();
    }

    [HttpPut("categories/{categoryId:guid}/reorder-items")]
    public async Task<IActionResult> ReorderItems(Guid categoryId, [FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderSkillItemsCommand(categoryId, orderedIds));
        await _cacheStore.EvictByTagAsync("skills", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/educations")]
public class AdminEducationsController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminEducationsController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await SendQueryAsync(new GetAdminEducationsQuery());
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminEducationByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEducationCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("educations", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEducationCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("educations", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteEducationCommand(id));
        await _cacheStore.EvictByTagAsync("educations", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangeEducationStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("educations", default);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderEducationsCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("educations", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/publications")]
public class AdminPublicationsController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminPublicationsController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await SendQueryAsync(new GetAdminPublicationsQuery());
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminPublicationByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePublicationCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("publications", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublicationCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("publications", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeletePublicationCommand(id));
        await _cacheStore.EvictByTagAsync("publications", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangePublicationStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("publications", default);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderPublicationsCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("publications", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/certifications")]
public class AdminCertificationsController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminCertificationsController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await SendQueryAsync(new GetAdminCertificationsQuery());
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminCertificationByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificationCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("certifications", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCertificationCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("certifications", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteCertificationCommand(id));
        await _cacheStore.EvictByTagAsync("certifications", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangeCertificationStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("certifications", default);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await SendCommandAsync(new ReorderCertificationsCommand(orderedIds));
        await _cacheStore.EvictByTagAsync("certifications", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/blog")]
public class AdminBlogController : BaseApiController
{
    private readonly IOutputCacheStore _cacheStore;

    public AdminBlogController(IOutputCacheStore cacheStore)
    {
        _cacheStore = cacheStore;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] ContentStatus? status = null)
    {
        var list = await SendQueryAsync(new GetAdminBlogPostsQuery(page, pageSize, status));
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminBlogPostByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogArticleCommand command)
    {
        var created = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("blog", default);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogArticleCommand command)
    {
        if (id != command.Id) return BadRequest(new { message = "Id in path does not match body." });
        var updated = await SendCommandAsync(command);
        await _cacheStore.EvictByTagAsync("blog", default);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteBlogArticleCommand(id));
        await _cacheStore.EvictByTagAsync("blog", default);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto)
    {
        await SendCommandAsync(new ChangeBlogArticleStatusCommand(id, dto.Status));
        await _cacheStore.EvictByTagAsync("blog", default);
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/messages")]
public class AdminMessagesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] MessageStatus? status = null)
    {
        var list = await SendQueryAsync(new GetAdminContactMessagesQuery(page, pageSize, status));
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminContactMessageByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeMessageStatusDto dto)
    {
        await SendCommandAsync(new ChangeContactMessageStatusCommand(id, dto.Status));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteContactMessageCommand(id));
        return NoContent();
    }
}

[Authorize(Policy = "AdminPolicy")]
[Route("api/v1/admin/media")]
public class AdminMediaController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var list = await SendQueryAsync(new GetAdminMediaAssetsQuery(page, pageSize));
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await SendQueryAsync(new GetAdminMediaAssetByIdQuery(id));
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string folder = "general", [FromForm] string? altText = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });

        using var stream = file.OpenReadStream();
        var command = new UploadMediaAssetCommand(
            stream,
            file.FileName,
            file.ContentType,
            folder,
            altText
        );

        var result = await SendCommandAsync(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/alt-text")]
    public async Task<IActionResult> UpdateAltText(Guid id, [FromBody] UpdateAltTextDto dto)
    {
        await SendCommandAsync(new UpdateMediaAssetAltTextCommand(id, dto.AltText));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await SendCommandAsync(new DeleteMediaAssetCommand(id));
        return NoContent();
    }
}

public record ChangeStatusDto(ContentStatus Status);
public record ChangeMessageStatusDto(MessageStatus Status);
public record UpdateAltTextDto(string AltText);
