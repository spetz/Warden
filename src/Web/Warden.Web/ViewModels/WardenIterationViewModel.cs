using System;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class WardenIterationViewModel
    {
        public Guid OrganizationId { get; set; }
        public Guid WardenId { get; set; }
        public WardenIterationDto Iteration { get; set; }
    }
}