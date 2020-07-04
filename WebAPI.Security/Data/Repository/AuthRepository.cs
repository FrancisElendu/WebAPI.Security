using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using WebAPI.Security.Data.Repository.IRepository;
using WebAPI.Security.Models;
using WebAPI.Security.Models.Dtos;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;

namespace WebAPI.Security.Data.Repository
{
    public class AuthRepository : Repository<UserManagerResponse<UserForRegisterDto>>, IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private byte[] passwordHash, passwordSalt, emailTokenSalt;

        public AuthRepository(ApplicationDbContext db, IConfiguration config) : base(db) 
        {
            _db = db;
            _config = config;
        }
        public async Task<UserManagerResponse<UserForRegisterDto>> RegisterUserAsync(User userForRegisterDto, IMailRepository mailRepository, string password)       
        {
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            userForRegisterDto.PasswordHash = passwordHash;
            userForRegisterDto.PasswordSalt = passwordSalt;

            // Generate email token and send confirmation email
            GenerateEmailConfirmationToken(out byte[] emailTokenHash, out byte[] emailTokenSalt);

            var userToCreate = new User
            {
                UserName = userForRegisterDto.UserName,
                Email = userForRegisterDto.Email,
                PasswordSalt = userForRegisterDto.PasswordSalt,
                PasswordHash = userForRegisterDto.PasswordHash,
                EmailTokenSalt = emailTokenSalt,
                Role = "Admin",
                //UserRole = userForRegisterDto.Role
            };

            //Register new User
            await _db.Users.AddAsync(userToCreate);
            int result = await _db.SaveChangesAsync();
            if (result > 0)
            {
                var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == userForRegisterDto.Email);

                // Generate email token and send confirmation email

                //GenerateEmailConfirmationToken(out byte[] emailTokenHash, out byte[] emailTokenSalt);

                var confirmEmailToken = Encoding.UTF8.GetString(emailTokenHash);

                var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);

                var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                string url = $"{_config["AppUrl"]}/api/auth/confirmemail?userId={user.Id}&token={validEmailToken}";

                await mailRepository.SendEmailAsync(user.Email, "Please Confirm your email", $"<h2>Welcome to Auth Demo</h2>" +
                    $"<p>Please confirm your email by <a href='{url}'>clicking here</a></p>");
            }

            var userToDto = new UserForRegisterDto
            {
                UserName = userToCreate.UserName,
                Email = userToCreate.Email
            };

            return new UserManagerResponse<UserForRegisterDto>
            {
                Message = "User Authentication Successful",
                IsSuccess = true,
                SingleData = userToDto
            };
        }

        public async Task<UserManagerResponse<UserForRegisterDto>> ConfirmEmailAsync(string userName, string token)
        {
            if (!await UserExists(userName))
            {
                return new UserManagerResponse<UserForRegisterDto>
                {
                    Message = "User not found.",
                    IsSuccess = false
                };
            }
            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
            var decodedToken = WebEncoders.Base64UrlDecode(token);
            // I need to retrieve the emailTokenSalt from Db and assign value to emailTokenSalt variable

            //if (!VerifyEmailConfirmationToken(decodedToken, emailTokenSalt))
            if (!VerifyEmailConfirmationToken(decodedToken, user.EmailTokenSalt))
                return new UserManagerResponse<UserForRegisterDto>
                {
                    Message = "User email not confirmed.",
                    IsSuccess = false
                };

            var userToDto = new UserForRegisterDto
            {
                UserName = user.UserName,
                Email = user.Email
            };
            return new UserManagerResponse<UserForRegisterDto>
            {
                SingleData = userToDto,
                Message = "User email confirmed Suceessfully.",
                IsSuccess = true
            };
        }

        public async Task<bool> UserExists(string userName)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return true;
            
            return false;
        }

        public async Task<UserManagerResponse<UserForRegisterDto>> Authenticate(UserForRegisterDto userForRegisterDto, string password)
        {
            if (password != userForRegisterDto.ConfirmPassword)
                return new UserManagerResponse<UserForRegisterDto>
                {
                    Message = "Password and Confirm Password does not match",
                    IsSuccess = false
                };
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userForRegisterDto.UserName);
            if (user == null)
                return null;
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            //if user was found, generate a JWT token
            var generatedToken = GenerateJwToken(user);

            var userToAuthenticate = new UserForRegisterDto
            {
                UserName = user.UserName,
                Email = user.Email
            };

            return new UserManagerResponse<UserForRegisterDto>
            {
                Message = "User Authentication Successful",
                IsSuccess = true,
                ExpiredDate = generatedToken.TokenExpiredDate,
                Token = generatedToken.Token,
                SingleData = userToAuthenticate
            };

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i=0; i<computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }
            return true;
        }

        private TokenCredentials GenerateJwToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Email", user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var expireDate = token.ValidTo;
            string tokenToSend = tokenHandler.WriteToken(token);

            return new TokenCredentials
            {
                Token = tokenToSend,
                TokenExpiredDate = expireDate
            };
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

        }

        //private string GenerateEmailConfirmationToken()
        //{
        //    var hash = new StringBuilder();
        //    byte[] secretKey = Encoding.UTF8.GetBytes(_config.GetSection("AppSecret:secretKey").Value);
        //    byte[] secretText = Encoding.UTF8.GetBytes(_config.GetSection("AppSecret:secretText").Value);
        //    using (var hmac = new HMACSHA512(secretKey))
        //    {
        //        byte[] hashValue = hmac.ComputeHash(secretText);
        //        foreach (var theByte in hashValue)
        //        {
        //            hash.Append(theByte.ToString("x2"));
        //        }
        //    }

        //    return hash.ToString();
        //}

        private void GenerateEmailConfirmationToken(out byte[] emailTokenHash, out byte[] emailTokenSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                emailTokenSalt = hmac.Key;
                emailTokenHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_config.GetSection("AppSecret:secretText").Value));
            }
        }

        private bool VerifyEmailConfirmationToken(byte[] emailTokenHash, byte[] emailTokenSalt)
        {
            using (var hmac = new HMACSHA512(emailTokenSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(_config.GetSection("AppSecret:secretText").Value));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != emailTokenHash[i]) return false;
                }
            }
            return true;
        }
    }
}
