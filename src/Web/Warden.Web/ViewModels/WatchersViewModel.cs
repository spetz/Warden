using System;
using System.Collections.Generic;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class WatchersViewModel
    {
        public Guid OrganizationId { get; set; }
        public Guid WardenId { get; set; }
        public IEnumerable<WatcherDto> Watchers { get; set; }
    }
}