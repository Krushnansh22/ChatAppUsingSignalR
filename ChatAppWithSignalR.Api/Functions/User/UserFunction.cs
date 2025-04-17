using ChatAppWithSignalR.Api.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatAppWithSignalR.Api.Functions.User
{
    public class UserFunction : IUserFunction
    {
        private readonly ChatAppContext _chatAppContext;

        public UserFunction(ChatAppContext chatAppContext)
        {
            _chatAppContext = chatAppContext;
        }

        public User? Authenticate(string loginId, string password)
        {
            try
            {
                // Trim the loginId and perform a case-insensitive lookup.
                loginId = loginId.Trim();

                var entity = _chatAppContext.TblUsers
                    .SingleOrDefault(x => x.LoginId.ToLower() == loginId.ToLower());

                if (entity == null)
                    return null;

                // Simply compare the plain text password.
                if (entity.Password != password)
                    return null;

                var token = GenerateJwtToken(entity);

                return new User
                {
                    Id = entity.Id,
                    UserName = entity.UserName,
                    AvatarSourceName = entity.AvatarSourceName,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                // Optionally log the exception for further debugging.
                return null;
            }
        }

        public User GetUserById(int id)
        {
            var entity = _chatAppContext.TblUsers.FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return new User();

            var awayDuration = entity.IsOnline ? "" : Utilities.CalcAwayDuration(entity.LastLogonTime);
            return new User
            {
                Id = entity.Id,
                UserName = entity.UserName,
                AvatarSourceName = entity.AvatarSourceName,
                IsOnline = entity.IsOnline,
                LastLogonTime = entity.LastLogonTime,
                IsAway = !string.IsNullOrEmpty(awayDuration),
                AwayDuration = awayDuration
            };
        }

        private string GenerateJwtToken(TblUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // In production, secure this key appropriately using configuration or environment variables.
            var key = Encoding.ASCII.GetBytes("1234567890123456");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString())
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}