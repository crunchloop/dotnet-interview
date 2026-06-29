using MediatR;
using Microsoft.AspNetCore.Mvc;
using Todo.Application.BulkCompletion.Commands.CancelBulkCompletion;
using Todo.Application.BulkCompletion.Commands.StartBulkCompletion;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.BulkCompletion.Queries.GetBulkJobProgress;

namespace Todo.Web.Controllers
{
    [Route("api/todo-items")]
    [ApiController]
    public class BulkCompletionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BulkCompletionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/todo-items/5/bulk-complete
        [HttpPost("{todoListId}/bulk-complete")]
        public async Task<IActionResult> BulkComplete(long todoListId)
        {
            var jobId = await _mediator.Send(
                new StartBulkCompletionCommand { TodoListId = todoListId }
            );

            return AcceptedAtAction(nameof(GetProgress), new { jobId }, new { jobId });
        }

        // GET: api/todo-items/bulk-jobs/{jobId}/progress
        [HttpGet("bulk-jobs/{jobId}/progress")]
        public async Task<ActionResult<JobProgressDto>> GetProgress(Guid jobId)
        {
            var progress = await _mediator.Send(new GetBulkJobProgressQuery { JobId = jobId });

            if (progress == null)
            {
                return NotFound();
            }

            return Ok(progress);
        }

        // DELETE: api/todo-items/bulk-jobs/{jobId}
        [HttpDelete("bulk-jobs/{jobId}")]
        public async Task<IActionResult> Cancel(Guid jobId)
        {
            var cancelled = await _mediator.Send(new CancelBulkCompletionCommand { JobId = jobId });

            return cancelled ? Accepted() : NotFound();
        }
    }
}
