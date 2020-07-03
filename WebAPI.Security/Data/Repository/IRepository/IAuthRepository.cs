using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Security.Models;
using WebAPI.Security.Models.Dtos;

namespace WebAPI.Security.Data.Repository.IRepository
{
    public interface IAuthRepository : IRepository<UserManagerResponse<UserForRegisterDto>>
    {
        //Task<UserManagerResponse<UserForRegisterDto>> RegisterUserAsync(UserForRegisterDto userForRegisterDto, IMailRepository mailRepository, string password);
        ////Task<UserManagerResponse<UserForRegisterDto>> RegisterUserAsync(IMailRepository mailRepository, string userName, string password);


        //Task<bool> UserExists(UserForRegisterDto userForRegisterDto, string userName, string password);
        Task<bool> UserExists(string userName);

        //Task<UserManagerResponse<UserForRegisterDto>> Authenticate(UserForRegisterDto userForRegisterDto, string userName, string password);
        Task<UserManagerResponse<UserForRegisterDto>> Authenticate(UserForRegisterDto userForRegisterDto, string password);


        //Task<UserManagerResponse<UserForRegisterDto>> ConfirmEmailAsync(string userId, string token);

        //Task<UserManagerResponse<UserForLoginDto>> LoginUserAsync(UserForLoginDto userForLoginDto, IMailRepository mailRepository);
        //Task<UserManagerResponse<UserForLoginDto>> ForgetPasswordAsync(string email, IMailRepository mailRepository);

        //Task<UserManagerResponse<ResetPasswordDto>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        
    }
}
