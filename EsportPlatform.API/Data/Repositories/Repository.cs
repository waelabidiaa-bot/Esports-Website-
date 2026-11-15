using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;

namespace EsportPlatform.API.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() => 
        await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(int id) => 
        await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity) => 
        await _dbSet.AddAsync(entity);

    public void Update(T entity) => 
        _dbSet.Update(entity);

    public void Delete(T entity) => 
        _dbSet.Remove(entity);

        public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
    
}