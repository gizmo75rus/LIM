using LIM.ApplicationCore.BaseObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using LIM.Infrastructure.Data;
using LIM.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LIM.UnitTests.Infrastructure.Data;

/// <summary>
/// Тестовый класс для EfRepository
/// Использует InMemory Database — быстрые Unit-тесты
/// </summary>
public class EfRepositoryTests : IDisposable
{
    private readonly TestAppDbContext _context;
    private readonly EfRepository _repository;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TestAppDbContext>> _loggerContextMock;
    private readonly Mock<ILogger<EfRepository>> _loggerRepositoryMock;

    #region Init test

    public EfRepositoryTests()
    {
        // 1. Настраиваем InMemory Database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        // 2. Mock ICurrentUserService для аудита
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(x => x.UserName).Returns("test-user");
        _currentUserServiceMock.Setup(x => x.UserId).Returns("12345");
        
        // 3. Mock логгера
        _loggerContextMock = new Mock<ILogger<TestAppDbContext>>();
        _loggerRepositoryMock = new Mock<ILogger<EfRepository>>();
        
        // 4. Создаем контекст и репозиторий
        _context = new TestAppDbContext(options, _currentUserServiceMock.Object, _loggerContextMock.Object);
        _repository = new EfRepository(_context, _loggerRepositoryMock.Object);
    }

    #endregion
    
    
    [Fact]
    public async Task AddAsync_ValidEntity_Returns1AndSetsAuditFields()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Name = "Test Item", 
            Value = 42 
        };
    
        // Act
        var result = await _repository.AddAsync(entity, CancellationToken.None);
        var saved = await _repository.Record<TestEntity>().FirstOrDefaultAsync();
    
        // Assert
        Assert.Equal(1, result); // SaveChangesAsync вернул 1 запись
        Assert.NotNull(saved);
        Assert.Equal("Test Item", saved.Name);
        Assert.Equal(42, saved.Value);
        Assert.NotEqual(default, saved.CreatedAt);
        Assert.Equal("test-user", saved.CreatedBy);
        Assert.Equal("test-user", saved.UpdatedBy);
    }
    
    [Fact]
    public async Task AddAsync_Collection_ReturnsCountAndAddsAll()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Name = "Item 1", Value = 1 },
            new TestEntity { Name = "Item 2", Value = 2 },
            new TestEntity { Name = "Item 3", Value = 3 }
        };
    
        // Act
        var result = await _repository.AddAsync(entities, CancellationToken.None);
        var count = await _repository.Record<TestEntity>().CountAsync();
    
        // Assert
        Assert.Equal(3, result);
        Assert.Equal(3, count);
    }
    
    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesFieldsAndSetsUpdatedAudit()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original", Value = 10 };
        await _repository.AddAsync(entity, CancellationToken.None);
    
        // Очищаем трекинг чтобы получить свежую копию
        _context.ChangeTracker.Clear();
    
        // Act
        entity.Name = "Updated";
        entity.Value = 20;
        var result = await _repository.UpdateAsync(entity, CancellationToken.None);
    
        var updated = await _repository.Record<TestEntity>().FirstOrDefaultAsync();
    
        // Assert
        Assert.Equal(1, result);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal(20, updated.Value);
        Assert.Equal("test-user", updated.UpdatedBy);
        Assert.NotNull(updated.UpdatedAt);
    }
    
    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        var entity = new TestEntity { Name = "To Delete", Value = 99 };
        await _repository.AddAsync(entity, CancellationToken.None);
    
        var beforeCount = await _repository.Record<TestEntity>().CountAsync();
        Assert.Equal(1, beforeCount);
    
        // Act
        var result = await _repository.DeleteAsync(entity, CancellationToken.None);
        var afterCount = await _repository.Record<TestEntity>().CountAsync();
    
        // Assert
        Assert.Equal(1, result);
        Assert.Equal(0, afterCount);
    }
    
    [Fact]
    public async Task Record_ReturnsIQueryable_WithAsNoTracking()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Name = "Apple", Value = 5 },
            new TestEntity { Name = "Banana", Value = 3 },
            new TestEntity { Name = "Cherry", Value = 8 }
        };
        await _repository.AddAsync(entities, CancellationToken.None);
    
        // Act - LINQ запрос поверх IQueryable
        var results = await _repository.Record<TestEntity>()
            .Where(x => x.Value > 4)
            .OrderBy(x => x.Name)
            .ToListAsync();
    
        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Apple", results[0].Name);
        Assert.Equal("Cherry", results[1].Name);
    
        // Проверка AsNoTracking - EntityState должен быть Detached
        var entry = _context.Entry(results.First());
        Assert.Equal(EntityState.Detached, entry.State);
    }
    
    #region TestInfrastructure

    /// <summary>
    /// Тестовая сущность для репозитория
    /// </summary>
    public class TestEntity : BaseEntity<Guid>, IJournaledEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    
        // IJournaledEntity
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
    {
        public void Configure(EntityTypeBuilder<TestEntity> builder)
        {
            builder.ToTable("TestEntities");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        }
    }
    
    public class TestAppDbContext : AppDbContext
    {
        public TestAppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService, ILogger<AppDbContext> logger) : base(options, currentUserService, logger)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
    
    #endregion

    
    public void Dispose()
    {
        _context.Dispose();
    }
}