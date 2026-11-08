using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevFreela.Core.Repositories;

namespace DevFreela.Infrastructure.Persistence
{
    public interface IUnitOfWork
    {
        IProjectRepository Projects { get; }
        IUserRepository Users { get; }
        ISkillRepository Skills { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
    }
}