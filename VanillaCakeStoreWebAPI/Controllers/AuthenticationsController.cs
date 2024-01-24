using AutoMapper;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using BusinessObject.Models;
using BusinessObject.Mail;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using VanillaCakeStoreWebAPI.DTO.Authentication;

namespace eBookStoreWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationsController : ControllerBase
    {
        private readonly VanillaCakeStoreContext _context;
        public IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public AuthenticationsController(VanillaCakeStoreContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService= emailService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginDTO user)
        {
            if (user != null && user.Email != null && user.Password != null)
            {
                var account = await _context.Accounts.Include(a => a.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Email == user.Email && u.Password == user.Password);

                if (account != null)
                {
                    var result = await this.GenerateToken(account.AccountId, false);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Invalid credentials!");
                }
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<TokenDTO> GenerateToken(int accountId, bool refresh)
        {
            var acc = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("AccountId", acc.AccountId.ToString()),
                        new Claim("Password", acc.Password),
                        new Claim("Email", acc.Email),
                        new Claim("CustomerId", acc.CustomerId != null ? acc.CustomerId : ""),
                        new Claim("EmployeeId", acc.EmployeeId.ToString()),
                        new Claim("Role", acc.Role.ToString())
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: signIn);

            var refreshToken = await this.GenerateRefreshToken();
            if (refresh)
            {
                var reToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.AccountId == acc.AccountId);
                reToken.RefreshToken1 = refreshToken;
                _context.Update<RefreshToken>(reToken);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (acc.RefreshTokens != null && acc.RefreshTokens.Any())
                {
                    var reToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.AccountId == acc.AccountId);
                    _context.RefreshTokens.Remove(reToken);
                    await _context.SaveChangesAsync();
                }

                var reFreshToken = new RefreshToken
                {
                    AccountId = acc.AccountId,
                    CreatedDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddMinutes(15),
                    RefreshToken1 = refreshToken
                };
                _context.RefreshTokens.Add(reFreshToken);
                await _context.SaveChangesAsync();
            }


            var result = new TokenDTO
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken
            };
            return result;
        }

        private async Task<string> GenerateRefreshToken()
        {
            var secureRandomBytes = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            await System.Threading.Tasks.Task.Run(() => randomNumberGenerator.GetBytes(secureRandomBytes));
            var refreshToken = Convert.ToBase64String(secureRandomBytes);
            return refreshToken;
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            if (refreshTokenRequest == null || string.IsNullOrEmpty(refreshTokenRequest.RefreshToken) || refreshTokenRequest.AccountId == 0)
            {
                return BadRequest();
            }

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.AccountId == refreshTokenRequest.AccountId);
            if (refreshToken == null)
            {
                return NotFound("Refresh Token not found!");
            }
            if (!refreshToken.RefreshToken1.Equals(refreshTokenRequest.RefreshToken))
            {
                return BadRequest("Refresh Token isn't valid!");
            }
            if (refreshToken.ExpiryDate < DateTime.Now)
            {
                return BadRequest("Refresh Token expired!"); ;
            }

            var result = await this.GenerateToken(refreshTokenRequest.AccountId, true);

            return Ok(result);
        }
        

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Logout()
        {
            var header = Request.Headers["Authorization"];
            var token = header[0].Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var accountId = Convert.ToInt32(jwt.Claims.First(claim => claim.Type == "AccountId").Value);

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(o => o.AccountId == accountId);
            if (refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            Account account = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Email.Equals(registerDTO.Email));

            if (account != null)
            {
                return BadRequest("Email existed!");
            }
            else
            {
                if (registerDTO.Password.Equals(registerDTO.RePassword))
                {
                    string cusId = await CreateCustomerId();
                    Customer customer = new Customer()
                    {
                        CustomerId = cusId,
                        ContactName = registerDTO.ContactName,
                        CompanyName = registerDTO.CompanyName,
                        Address = registerDTO.Address,
                        ContactTitle = registerDTO.ContactTitle
                    };
                    await _context.AddAsync<Customer>(customer);
                    _context.SaveChanges();

                    Account acc = new Account()
                    {
                        CustomerId = cusId,
                        Email = registerDTO.Email,
                        Password = registerDTO.Password,
                        Role = 2
                    };
                    await _context.AddAsync<Account>(acc);
                    _context.SaveChanges();
                    return Ok("Regist successfully!");
                }
                else
                {
                    return BadRequest("Password doesn't match");
                }
            }
        }

        private async Task<string> CreateCustomerId()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            bool existed = true;
            string cusId = "";
            while (existed)
            {

                cusId = new string(Enumerable.Repeat(chars, 5)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                Customer customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId.Equals(cusId));
                if (customer == null)
                {
                    existed = false;
                }
            }
            return cusId;

        }
/*
        [HttpGet("[action]")]
        public IActionResult getClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var CustomerId = jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
            var EmployeeId = jwt.Claims.First(claim => claim.Type == "EmployeeId").Value;
            var AccountId = jwt.Claims.First(claim => claim.Type == "AccountId").Value;
            var Role = jwt.Claims.First(claim => claim.Type == "Role").Value;
            var Email = jwt.Claims.First(claim => claim.Type == "Email").Value;
            return Ok(new { CustomerId, EmployeeId, AccountId, Role, Email });
        }*/

        [HttpGet("[action]")]
        public IActionResult getAccountClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var CustomerId = jwt.Claims.First(claim => claim.Type == "CustomerId").Value;
            var EmployeeId = !jwt.Claims.First(claim => claim.Type == "EmployeeId").Value.Equals("") ?
                Convert.ToInt32(jwt.Claims.First(claim => claim.Type == "EmployeeId").Value) : 0;
            var AccountId = Convert.ToInt32(jwt.Claims.First(claim => claim.Type == "AccountId").Value);
            var Role = Convert.ToInt32(jwt.Claims.First(claim => claim.Type == "Role").Value);
            var Email = jwt.Claims.First(claim => claim.Type == "Email").Value;

            return Ok(new ClaimDTO
            {
                CustomerId = CustomerId,
                EmployeeId = EmployeeId,
                AccountId = AccountId,
                Role = Role,
                Email = Email
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            string newPass = this.GenerateRandomPassword(8);
            Account account = await _context.Accounts.FirstOrDefaultAsync(acc => acc.Email.Equals(email));

            if (account == null)
            {
                return BadRequest("Email doesn't existed!");
            }
            account.Password = newPass;
            _context.Accounts.Update(account);
            _context.SaveChanges();

            var emailDTO = new MailContent
            {
                To = email,
                Body = "<h1>This is your new password</h1> <p>" + newPass + "</p>",
                Subject = "Vanilla Cake Store Reset Password"
            };
            _emailService.SendMail(emailDTO);
            return Ok();
        }
        
        private string GenerateRandomPassword(int length)
        {
            string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            char[] chars = new char[length];
            byte[] data = new byte[length];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            for (int i = 0; i < length; i++)
            {
                chars[i] = AllowedChars[data[i] % AllowedChars.Length];
            }

            return new string(chars);
        }

        [HttpPost]
        public IActionResult SendMail(MailContent emailDTO)
        {
            _emailService.SendMail(emailDTO);
            return Ok();
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> ChangePass(ChangePassDTO password)
        {
            var header = Request.Headers["Authorization"];
            var token = header[0].Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var accountId = Convert.ToInt32(jwt.Claims.First(claim => claim.Type == "AccountId").Value);

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (account == null)
            {
                return BadRequest("Account doesn't exist!");
            }
            if (!account.Password.Equals(password.OldPassword))
            {
                return BadRequest("Old password incorrect!");
            }
            if (password.NewPassword.Equals(password.OldPassword))
            {
                return BadRequest("New password can't match old password!");
            }
            account.Password = password.NewPassword;
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
