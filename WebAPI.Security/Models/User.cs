using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Security.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        [NotMapped]
        public string Password { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[] EmailTokenSalt { get; set; }
        public string Role { get; set; }

    }
}
