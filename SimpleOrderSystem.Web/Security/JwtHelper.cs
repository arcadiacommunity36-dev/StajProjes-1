using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using SimpleOrderSystem.Entities.Models;
using Microsoft.IdentityModel.Tokens;

namespace SimpleOrderSystem.Web.Security
{
    public class JwtHelper
    {
        public SymmetricSecurityKey SecurityKey { get; }

        public JwtHelper()
        {
            var bytes = new byte[64];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }
            SecurityKey = new SymmetricSecurityKey(bytes);
        }

        public object CreateToken(UserView user)
        {
            var expiration = DateTime.UtcNow.AddHours(8);
            var token = new JwtSecurityToken(
                issuer: "SimpleOrderSystem",
                audience: "SimpleOrderSystem",
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) },
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256));

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration,
                user
            };
        }
    }
}
