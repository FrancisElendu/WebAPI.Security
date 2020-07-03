using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Security.Models.Dtos
{
    public class UserForRegisterDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        //public byte[] PasswordHash { get; set; }
        //public byte[] PasswordSalt { get; set; }

        public string ConfirmPassword { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

    }
}
