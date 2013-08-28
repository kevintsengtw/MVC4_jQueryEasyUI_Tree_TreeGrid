using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace jQueryEasyUI_Tree.Models.Repository
{
    public class DataRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// The context object for the database
        /// </summary>
        private DbContext _context { get; set; }

        /// <summary>
        /// Gets or sets the db set.
        /// </summary>
        /// <value>
        /// The db set.
        /// </value>
        private DbSet<TEntity> dbSet
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRepository{TEntity}"/> class.
        /// </summary>
        public DataRepository(DbContext dbcontext)
        {
            this._context = dbcontext;
            this.dbSet = this._context.Set<TEntity>();
        }

        /// <summary>
        /// Counts this instance.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return this._context.Set<TEntity>().Count();
        }

        /// <summary>
        /// Gets all records as an IEnumberable
        /// </summary>
        /// <returns>An IEnumberable object containing the results of the query</returns>
        public IQueryable<TEntity> GetAll()
        {
            return this._context.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// Finds a record with the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A collection containing the results of the query</returns>
        public IQueryable<TEntity> Find(Func<TEntity, bool> predicate)
        {
            return this._context.Set<TEntity>().Where(predicate).AsQueryable();
        }

        /// <summary>
        /// Firsts the or default.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public TEntity FirstOrDefault(Func<TEntity, bool> predicate)
        {
            return this._context.Set<TEntity>().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Determines whether the specified predicate is exists.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        ///   <c>true</c> if the specified predicate is exists; otherwise, <c>false</c>.
        /// </returns>
        public bool IsExists(Func<TEntity, bool> predicate)
        {
            return this._context.Set<TEntity>().FirstOrDefault(predicate) != null;
        }

        /// <summary>
        /// Deletes the specified entitiy
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this._context.Entry(entity).State = EntityState.Deleted;
            this._context.SaveChanges();
        }

        /// <summary>
        /// Deletes records matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        public void Delete(Func<TEntity, bool> predicate)
        {
            IEnumerable<TEntity> records = from x in this._context.Set<TEntity>().Where(predicate) select x;

            foreach (var record in records)
            {
                this._context.Entry(record).State = EntityState.Deleted;
                this._context.SaveChanges();
            }
        }

        /// <summary>
        /// Adds the specified entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            this._context.Entry(entity).State = EntityState.Added;
            this._context.SaveChanges();
        }

        /// <summary>
        /// Updates the specified instance.
        /// </summary>
        /// <param name="entity">The instance.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                this._context.Entry(entity).State = EntityState.Modified;
                this._context.SaveChanges();
            }
        }
    }
}