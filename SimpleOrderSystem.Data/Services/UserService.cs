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
    public class UserService
    {
        private readonly OrderDbContext db;

        public UserService(OrderDbContext db)
        {
            this.db = db;
        }

        public async Task<List<UserView>> GetAllAsync()
        {
            var users = await db.Users.AsNoTracking().OrderBy(x => x.FullName).ToListAsync();
            var result = new List<UserView>();
            foreach (var user in users)
                result.Add(await CreateViewAsync(user));
            return result;
        }

        public async Task<UserView> GetActiveUserAsync(int id)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Status);
            return user == null ? null : await CreateViewAsync(user);
        }

        public async Task<UserView> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                return null;

            var normalizedEmail = email.Trim().ToUpperInvariant();
            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email.ToUpper() == normalizedEmail && x.Status);
            if (user == null || !HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return await CreateViewAsync(user);
        }

        public async Task<UserView> CreateAsync(UserRequest request)
        {
            CheckPassword(request.Password);
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await CheckEmailAsync(request.Email, 0);
            var roleIds = await CheckRolesAsync(request.RoleIds);
            HashingHelper.CreatePasswordHash(request.Password, out var hash, out var salt);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Gender = request.Gender,
                PasswordHash = hash,
                PasswordSalt = salt,
                Status = request.Status
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            foreach (var roleId in roleIds)
                db.UserGroups.Add(new UserGroup { UserId = user.Id, GroupId = roleId });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return await CreateViewAsync(user);
        }

        public async Task UpdateAsync(int id, UserRequest request)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new BusinessException("The user was not found.", 404);

            await CheckEmailAsync(request.Email, id);
            var roleIds = await CheckRolesAsync(request.RoleIds);
            await CheckLastAdminAsync(user, request.Status, roleIds);
            if (!string.IsNullOrEmpty(request.Password))
            {
                CheckPassword(request.Password);
                HashingHelper.CreatePasswordHash(request.Password, out var hash, out var salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            user.FullName = request.FullName.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();
            user.Email = request.Email.Trim().ToLowerInvariant();
            user.Gender = request.Gender;
            user.Status = request.Status;

            var assignments = await db.UserGroups.IgnoreQueryFilters().Where(x => x.UserId == id).ToListAsync();
            foreach (var assignment in assignments)
            {
                if (roleIds.Contains(assignment.GroupId))
                {
                    assignment.IsDeleted = false;
                    assignment.Status = true;
                }
                else if (!assignment.IsDeleted)
                {
                    db.UserGroups.Remove(assignment);
                }
            }
            foreach (var roleId in roleIds)
            {
                if (!assignments.Any(x => x.GroupId == roleId))
                    db.UserGroups.Add(new UserGroup { UserId = id, GroupId = roleId });
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new BusinessException("The user was not found.", 404);
            await CheckLastAdminAsync(user, false, Array.Empty<int>());
            var assignments = await db.UserGroups.Where(x => x.UserId == id).ToListAsync();
            db.UserGroups.RemoveRange(assignments);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task<UserView> CreateViewAsync(User user)
        {
            var roles = await (from assignment in db.UserGroups.AsNoTracking()
                               join role in db.Groups.AsNoTracking() on assignment.GroupId equals role.Id
                               where assignment.UserId == user.Id && assignment.Status && role.Status
                               orderby role.GroupName
                               select role).ToListAsync();
            var roleIds = roles.Select(x => x.Id).ToArray();
            var permissions = await (from link in db.GroupClaims.AsNoTracking()
                                     join claim in db.OperationClaims.AsNoTracking() on link.ClaimId equals claim.Id
                                     where roleIds.Contains(link.GroupId) && link.Status && claim.Status
                                     select claim.Name).Distinct().ToArrayAsync();
            return new UserView
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Gender = user.Gender,
                RoleIds = roleIds,
                Roles = roles.Select(x => x.GroupName).ToArray(),
                Permissions = permissions,
                Status = user.Status
            };
        }

        private async Task CheckEmailAsync(string email, int id)
        {
            var value = email.Trim().ToUpperInvariant();
            if (await db.Users.AnyAsync(x => x.Id != id && x.Email.ToUpper() == value))
                throw new BusinessException("This email address is already in use.", 409);
        }

        private async Task<int[]> CheckRolesAsync(int[] requestedIds)
        {
            var ids = requestedIds.Distinct().ToArray();
            var count = await db.Groups.CountAsync(x => ids.Contains(x.Id) && x.Status);
            if (count != ids.Length || ids.Length == 0)
                throw new BusinessException("Select at least one active role.");
            return ids;
        }

        private async Task CheckLastAdminAsync(User user, bool status, int[] roleIds)
        {
            var admin = await db.Groups.FirstAsync(x => x.GroupName == "Admin" && x.Status);
            var wasAdmin = user.Status && await db.UserGroups.AnyAsync(x => x.UserId == user.Id && x.GroupId == admin.Id && x.Status);
            if (!wasAdmin || status && roleIds.Contains(admin.Id))
                return;

            var otherAdmin = await (from employee in db.Users
                                    join assignment in db.UserGroups on employee.Id equals assignment.UserId
                                    where employee.Id != user.Id && employee.Status && assignment.Status && assignment.GroupId == admin.Id
                                    select employee.Id).AnyAsync();
            if (!otherAdmin)
                throw new BusinessException("The last active administrator cannot be removed or deactivated.");
        }

        private static void CheckPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 128)
                throw new BusinessException("The password must contain 8 to 128 characters.");
        }
    }
}
