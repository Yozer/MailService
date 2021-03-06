﻿using System;
using MailService.Api.Dto;
using MediatR;

namespace MailService.Api.Commands
{
    public class PatchEmailCommand : IRequest<bool>
    {
        public Guid Id { get; }
        public CreateEmailDto Patch { get; }

        public PatchEmailCommand(Guid id, CreateEmailDto patch)
        {
            Id = id;
            Patch = patch;
        }
    }
}
