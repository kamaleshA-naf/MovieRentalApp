using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Interfaces;
using System.Linq.Expressions;

namespace MovieRentalApp.Repositories
{
    public class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly MovieContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(MovieContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // ── Add ───────────────────────────────────────────────────
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // ── Get By Id ─────────────────────────────────────────────
        public async Task<T?> GetByIdAsync(K id)
        {
            return await _dbSet.FindAsync(id);
        }

        // ── Get All ✅ AsNoTracking ────────────────────────────────
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        // ── Find ✅ AsNoTracking ───────────────────────────────────
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        // ── Exists ────────────────────────────────────────────────
        public async Task<bool> ExistsAsync(K id)
        {
            return await _dbSet.FindAsync(id) != null;
        }

        // ── Get All With Include ✅ AsNoTracking + multi-include ───
        public async Task<IEnumerable<T>> GetAllWithIncludeAsync(
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            foreach (var include in includes)
                query = query.Include(include);
            return await query.ToListAsync();
        }

        // ── GetQueryable ✅ DB-side pagination ────────────────────
        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsNoTracking().AsQueryable();
        }

        // ── Update ────────────────────────────────────────────────
        public async Task<T?> UpdateAsync(K id, T entity)
        {
            var existing = await _dbSet.FindAsync(id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }

        // ── Delete ────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(K id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}