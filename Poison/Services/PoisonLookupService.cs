﻿using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Models;
using Poison.Services.Interfaces;

namespace Poison.Services
{
    public class PoisonLookupService : IPoisonLookupService
    {
        #region Properties
        private readonly ApplicationDbContext _context;

        #endregion

        #region Constructor
        public PoisonLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion

        #region Get Project Priorities
        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get Ticket Priorities
        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get Ticket Statuses
        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Get Ticket Types
        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion    

        public async Task<int?> LookupNotificationTypeIdAsync(string typeName)
        {
            try
            {
                NotificationType? notificationType = await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == typeName);
                return notificationType!.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
