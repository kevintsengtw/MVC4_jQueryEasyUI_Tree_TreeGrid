using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jQueryEasyUI_Tree.Models.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);

        void Delete(TEntity entity);

        void Update(TEntity entity);

        
        int Count();

        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> Find(Func<TEntity, bool> predicate);

        TEntity FirstOrDefault(Func<TEntity, bool> predicate);

        bool IsExists(Func<TEntity, bool> predicate);

    }
}
