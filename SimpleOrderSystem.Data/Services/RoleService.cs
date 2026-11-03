using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SimpleOrderSystem.Data;
using SimpleOrderSystem.Entities.Models;
using SimpleOrderSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace SimpleOrderSystem.Data.Services
{
    public class RoleService
    {
        private readonly OrderDbContext db;
        private readonly string[] systemRoles = { "Admin", "User", "CustomerRepresentative" };

        public RoleService(OrderDbContext db)
        {
            this.db = db;
        }

        public Task<List<RoleView>> GetAllAsync()
        {
            return db.Groups.AsNoTracking().OrderBy(x => x.GroupName)
                .Select(x => new RoleView { Id = x.Id, Name = x.GroupName, Status = x.Status }).ToListAsync();
        }

        public async Task<RoleView> CreateAsync(RoleRequest request)
        {
            await CheckNameAsync(request.Name, 0);
            var role = new Group { GroupName = request.Name.Trim(), Status = request.Status };
            db.Groups.Add(role);
            await db.SaveChangesAsync();
            return new RoleView { Id = role.Id, Name = role.GroupName, Status = role.Status };
        }

        public async Task UpdateAsync(int id, RoleRequest request)
        {
            var role = await db.Groups.FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
                throw new BusinessException("The role was not found.", 404);
            if (systemRoles.Contains(role.GroupName) && (request.Name.Trim() != role.GroupName || !request.Status))
                throw new BusinessException("System roles cannot be renamed or deactivated.");
            await CheckNameAsync(request.Name, id);
            role.GroupName = request.Name.Trim();
            role.Status = request.Status;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var role = await db.Groups.FirstOrDefaultAsync(x => x.Id == id);
            if (role == null)
                throw new BusinessException("The role was not found.", 404);
            if (systemRoles.Contains(role.GroupName))
                throw new BusinessException("System roles cannot be deleted.");
            if (await db.UserGroups.AnyAsync(x => x.GroupId == id))
                throw new BusinessException("Remove this role from users before deleting it.");
            var permissions = await db.GroupClaims.Where(x => x.GroupId == id).ToListAsync();
            db.GroupClaims.RemoveRange(permissions);
            db.Groups.Remove(role);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task CheckNameAsync(string name, int id)
        {
            var value = name.Trim().ToUpperInvariant();
            if (await db.Groups.AnyAsync(x => x.Id != id && x.GroupName.ToUpper() == value))
                throw new BusinessException("A role with this name already exists.", 409);
        }
    }
}
