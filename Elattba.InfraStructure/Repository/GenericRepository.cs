using Elattba.Core.InterFaces;
using Elattba.InfraStructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Elattba.InfraStructure.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly El3atbaDbContext _context;

        public GenericRepository(El3atbaDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(T entity)
        {   
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
            }
        }
      
        public async Task<IReadOnlyList<T>> GetAllAsync() => await ListAsync();


        public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes) =>
            await ListAsync(null, true, includes);

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includes)
        {
            return await BuildQuery(disableTracking, includes).FirstOrDefaultAsync(predicate);
        }

        public async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            params Expression<Func<T, object>>[] includes)
        {
            var query = BuildQuery(disableTracking, includes);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
            await _context.Set<T>().AnyAsync(predicate);



        public async Task<T?> GetByIdAsync(int id)
        {
            var  entity = await _context.Set<T>().FindAsync(id);
            return entity;
        }

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            var query = _context.Set<T>().AsQueryable();

            foreach (var item in includes)
                query = query.Include(item);

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
            return entity;
        }

        public Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        private IQueryable<T> BuildQuery(bool disableTracking, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public Task<int> CountAsync()
        {
            return _context.Set<T>().CountAsync();
        }
    }
}
