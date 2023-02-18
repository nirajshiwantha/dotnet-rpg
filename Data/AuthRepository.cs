using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));
            if(user is null){
                response.Sucess = false;
                response.Message = "User not Found!";
            }

            else if(!VerifyPassworHash(password,user.PasswordHash, user.PasswordSalt)){
                response.Sucess = false;
                response.Message = "Incorrect Password!";
            }
            else {
                response.Data = CreateToken(user);
            }

            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            if(await UserExists(user.Username)){
                response.Sucess = false;
                response.Message = "User Already Exists!";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            response.Data = user.Id;
            return response;
        } 


        public async Task<bool> UserExists(string username)
        {
            if(await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower())){
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512()){
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPassworHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
                
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var appSettingToken = _configuration.GetSection("AppSettings:Token").Value;
            if(appSettingToken is null)
                throw new Exception("AppSettings Token is NULL");
            
            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(appSettingToken));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescripor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.Now.AddDays(1),
                Expires = DateTime.Now.AddMinutes(60),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescripor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ServiceResponse<int>> ResetPassword(User user, string oldPassword, string newPassword)
            {
            var response = new ServiceResponse<int>();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);

            if (currentUser == null)
            {
                response.Sucess = false;
                response.Message = "User not found.";
                return response;
            }

            if (!VerifyPassworHash(oldPassword, currentUser.PasswordHash, currentUser.PasswordSalt))
            {
                response.Sucess = false;
                response.Message = "Old password is incorrect.";
                return response;
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            currentUser.PasswordHash = passwordHash;
            currentUser.PasswordSalt = passwordSalt;

            _context.Users.Update(currentUser);
            await _context.SaveChangesAsync();

            response.Data = currentUser.Id;
            return response;
        }
        
    }
}