using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleOrderSystem.Entities;
using SimpleOrderSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace SimpleOrderSystem.Data
{
    public class DbInitializer
    {
        private readonly OrderDbContext _context;

        public DbInitializer(OrderDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync(string email, string password)
        {
            await _context.Database.EnsureCreatedAsync();
            _context.IsSeeding = true;

            try
            {
                if (await _context.Users.IgnoreQueryFilters().AnyAsync())
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
                {
                    throw new InvalidOperationException("Configure an initial admin email and a password of at least 8 characters.");
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    var rolePermissions = new Dictionary<string, string[]>
                    {
                        ["Admin"] = new[] { "Users", "Roles", "Customers", "Products", "Reports" },
                        ["User"] = new[] { "Products", "Reports" },
                        ["CustomerRepresentative"] = new[] { "Customers", "Products", "CreateOrders", "Reports" }
                    };

                    var permissionNames = new[] { "Users", "Roles", "Customers", "Products", "CreateOrders", "Reports" };
                    var groups = rolePermissions.Keys.Select(name => new Group { GroupName = name }).ToList();
                    var claims = permissionNames.Select(name => new OperationClaim
                    {
                        Name = name,
                        Alias = name,
                        Description = name
                    }).ToList();

                    _context.Groups.AddRange(groups);
                    _context.OperationClaims.AddRange(claims);
                    await _context.SaveChangesAsync();

                    foreach (var group in groups)
                    {
                        foreach (var name in rolePermissions[group.GroupName])
                        {
                            _context.GroupClaims.Add(new GroupClaim
                            {
                                GroupId = group.Id,
                                ClaimId = claims.Single(x => x.Name == name).Id
                            });
                        }
                    }

                    HashingHelper.CreatePasswordHash(password, out var hash, out var salt);
                    var admin = new User
                    {
                        FullName = "Administrator",
                        PhoneNumber = "0000000000",
                        Email = email.Trim().ToLowerInvariant(),
                        Gender = "Other",
                        PasswordHash = hash,
                        PasswordSalt = salt
                    };

                    _context.Users.Add(admin);
                    await _context.SaveChangesAsync();

                    _context.UserGroups.Add(new UserGroup
                    {
                        UserId = admin.Id,
                        GroupId = groups.Single(x => x.GroupName == "Admin").Id
                    });

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                _context.IsSeeding = false;
            }
        }
    }
}
