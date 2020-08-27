using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailService.Api.Commands;
using MailService.Api.Dto;
using MailService.Api.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MailService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmailsController(IMediator mediator) 
            => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<List<EmailDto>>> GetAll()
        {
            var result = await Send(new GetAllEmailsQuery());
            return result.ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmailDto>> GetOne(Guid id)
        {
            var email = await Send(new GetEmailQuery(id));
            if (email == null)
                return NotFound();
            return email;
        }

        [HttpGet("{id}/status")]
        public async Task<ActionResult<EmailStatusDto>> GetOneStatus(Guid id)
        {
            var result = await Send(new GetEmailQuery(id));
            if (result == null)
                return NotFound();

            return new EmailStatusDto
            {
                Status = result.Status
            };
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchEmail(Guid id, [FromBody] PatchEmailDto patch)
        {
            var foundEmailToUpdated = await Send(new PatchEmailCommand(id, patch));
            if (!foundEmailToUpdated)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/attachments")]
        public async Task<ActionResult> AddAttachments(string id, [FromBody] ICollection<IFormFile> attachments)
        {
            await Send(new AddAttachmentsToEmailCommand(id, attachments));
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<EmailDto>> AddEmail(EmailDto email)
        {
            var result = await Send(new CreateEmailCommand(email));
            return CreatedAtAction(nameof(GetOne), new {id = result.Id}, result);
        }

        // in case we expect that user can send large amount of emails
        // this could be handled asynchronously by some background worker
        // we could even dedicated queue with persistent messages
        [HttpPost("send-pending")]
        public async Task<ActionResult> SendPendingEmails()
        {
            await Send(new SendPendingEmailsCommand());
            return Ok();
        }

        private Task<TResult> Send<TResult>(IRequest<TResult> command)
            => _mediator.Send(command);
    }
}
