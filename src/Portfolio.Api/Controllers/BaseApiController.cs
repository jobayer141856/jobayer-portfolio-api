using Cortex.Mediator;
using Cortex.Mediator.Commands;
using Cortex.Mediator.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected Task<TResult> SendQueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct = default) =>
        Mediator.SendQueryAsync(query, ct);

    protected Task<TResult> SendCommandAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default) =>
        Mediator.SendCommandAsync(command, ct);

    protected Task SendCommandAsync(ICommand command, CancellationToken ct = default) =>
        Mediator.SendCommandAsync(command, ct);
}
