using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Security.Data.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthRepository AuthServices { get; }
        IMailRepository MailServices { get; }

        void Save();
    }
}
