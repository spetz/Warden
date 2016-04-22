using System;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class WardenViewModel
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public PagedResult<WardenIterationDto> Iterations { get; set; }
    }
}