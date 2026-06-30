using MediatR;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.Sync.Commands.RunSync;
using Todo.Application.Sync.Models;

namespace Todo.Web.Controllers
{
    [Route("api/sync")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SyncController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/sync
        [HttpPost]
        public async Task<ActionResult<SyncResult>> Sync()
        {
            return Ok(await _mediator.Send(new RunSyncCommand()));
        }
    }
}
