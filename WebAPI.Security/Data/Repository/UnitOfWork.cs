using Microsoft.Extensions.Configuration;
using WebAPI.Security.Data.Repository.IRepository;
using WebAPI.Security.Data.Repository;

namespace WebAPI.Security.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private IAuthRepository _authRepo;
        private IMailRepository _mailRepo;

        public UnitOfWork(ApplicationDbContext db, IConfiguration config) 

        {
            _db = db;
            _config = config;
        }

        public IAuthRepository AuthServices
        {
            get
            {
                if (_authRepo == null)
                    _authRepo = new AuthRepository(_db, _config);
                return _authRepo;
            }
        }

        public IMailRepository MailServices
        {
            get
            {
                if (_mailRepo == null)
                    _mailRepo = new MailRepository(_db, _config);
                return _mailRepo;
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
