﻿using Poison.Models;

namespace Poison.Services.Interfaces
{
    public interface IPoisonLookupService
    {
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
        public Task<List<TicketStatus>> GetTicketStatusesAsync();
        public Task<List<TicketType>> GetTicketTypesAsync();
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<int?> LookupNotificationTypeIdAsync(string typeName);

    }
}
