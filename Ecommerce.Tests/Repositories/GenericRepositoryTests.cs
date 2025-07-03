using Ecommerce.Repository.implementation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Ecommerce.Tests.Repositories
{
    public class GenericRepositoryTests
    {
        private readonly Mock<DbSet<TestEntity>> _mockDbSet;
        private readonly Mock<Ecommerce.Repository.Models.EcommerceContext> _mockContext;
        private readonly GenericRepository<TestEntity> _repository;

        public GenericRepositoryTests()
        {
            _mockDbSet = new Mock<DbSet<TestEntity>>();
            _mockContext = new Mock<Ecommerce.Repository.Models.EcommerceContext>();
            _mockContext.Setup(c => c.Set<TestEntity>()).Returns(_mockDbSet.Object);
            _repository = new GenericRepository<TestEntity>(_mockContext.Object);
        }

        public class TestEntity
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
        {
            // Arrange
            TestEntity testEntity = new TestEntity { Id = 1, Name = "Test" };
            _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(testEntity);

            // Act
            TestEntity result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            // Arrange
            TestEntity testEntity = new TestEntity { Id = 2, Name = "New Entity" };
            _mockDbSet.Setup(m => m.AddAsync(testEntity, default))
                .Returns(ValueTask.FromResult(default(EntityEntry<TestEntity>))); // Return ValueTask
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            await _repository.AddAsync(testEntity);

            // Assert
            _mockDbSet.Verify(m => m.AddAsync(testEntity, default), Times.Once());
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task AddAsync_DbException_ThrowsExceptionWithMessage()
        {
            // Arrange
            var entity = new TestEntity();
            string exceptionMessage = "Database error";
            _mockDbSet.Setup(d => d.AddAsync(entity, default))
                .Returns(ValueTask.FromResult(default(EntityEntry<TestEntity>))); // Return ValueTask
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ThrowsAsync(new Exception(exceptionMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _repository.AddAsync(entity));
            Assert.Equal($"Error adding entity: {exceptionMessage}", exception.Message);
            _mockDbSet.Verify(d => d.AddAsync(entity, default), Times.Once());
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once());
        }
    }
}