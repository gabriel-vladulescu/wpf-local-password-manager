using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountManager.Core.Interfaces
{
    /// <summary>
    /// Generic repository pattern interface for data access operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        Task<T> GetAsync();
        Task<bool> SaveAsync(T entity);
        Task<bool> ExistsAsync();
        void InvalidateCache();
        event EventHandler<T> DataChanged;
    }
}