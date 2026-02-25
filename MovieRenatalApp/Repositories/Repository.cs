using Microsoft.EntityFrameworkCore;
using MovieRentalApp.Contexts;
using MovieRentalApp.Interfaces;

namespace MovieRentalApp.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {
        protected readonly MovieContext _context;

        protected Repository(MovieContext context)
        {
            _context = context;
        }

        // Each child class tells us HOW to find by key
        protected abstract Task<T?> GetByKey(K key);

        public async Task<T> Add(T item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<T?> Get(K key)
        {
            return await GetByKey(key);
        }

        public async Task<IEnumerable<T>?> GetAll()
        {
            var items = await _context.Set<T>().ToListAsync();
            return items.Any() ? items : null;
        }

        public async Task<T?> Update(K key, T item)
        {
            var existingItem = await GetByKey(key);
            if (existingItem == null) return null;

            _context.Entry(existingItem).CurrentValues.SetValues(item);
            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<T?> Delete(K key)
        {
            var item = await GetByKey(key);
            if (item == null) return null;

            _context.Remove(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }
}