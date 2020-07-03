using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Security.Data.Repository.IRepository;

namespace WebAPI.Security.Data.Repository
{
    public class MailRepository : IMailRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        public MailRepository(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var apiKey = _config["SendGridAPIKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("test@example.com", "JWT Auth Demo");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
