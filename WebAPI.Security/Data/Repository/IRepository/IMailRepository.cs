using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Security.Data.Repository.IRepository
{
    public interface IMailRepository
    {
        Task SendEmailAsync(string toEmail, string subject, string content);
    }
}
