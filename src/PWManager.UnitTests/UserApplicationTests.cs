using Moq;
using PWManager.Application;
using PWManager.Application.DataContracts;
using PWManager.Domain.DataContracts.Repository;
using PWManager.Domain.Model;
using PWManager.UnitTests.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PWManager.UnitTests;

public class UserApplicationTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly IUserApplication _sut;

    public UserApplicationTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _sut = new UserApplication(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var users = UserMock.GetUsers();
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        var result = await _sut.GetAllUsersAsync();

        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Equal(3, list.Count);
        Assert.Equal(UserMock.Id1, list[0].Id);
        Assert.Equal("github.com", list[0].Site);
        Assert.Equal("google.com", list[1].Site);
        Assert.Equal("amazon.com", list[2].Site);
    }

    [Fact]
    public async Task AddUserAsync_CallsRepositoryAddOnce()
    {
        var user = UserMock.GetSingleUser();
        _repositoryMock
            .Setup(r => r.AddAsync(user))
            .Returns(Task.CompletedTask);

        await _sut.AddUserAsync(user);

        _repositoryMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenExists_ReturnsMatchingUser()
    {
        var expected = UserMock.GetSingleUser();
        _repositoryMock
            .Setup(r => r.GetUserByIdAsync(UserMock.Id1))
            .ReturnsAsync(expected);

        var result = await _sut.GetUserByIdAsync(UserMock.Id1);

        Assert.NotNull(result);
        Assert.Equal(UserMock.Id1, result.Id);
        Assert.Equal("github.com", result.Site);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenNotFound_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetUserByIdAsync(nonExistentId))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserByIdAsync(nonExistentId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_CallsRepositoryUpdateOnce()
    {
        var user = UserMock.GetSingleUser();
        _repositoryMock
            .Setup(r => r.UpdateAsync(user))
            .Returns(Task.CompletedTask);

        await _sut.UpdateUserAsync(user);

        _repositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_CallsRepositoryDeleteOnce()
    {
        var user = UserMock.GetSingleUser();
        _repositoryMock
            .Setup(r => r.DeleteAsync(user))
            .Returns(Task.CompletedTask);

        await _sut.DeleteUserAsync(user);

        _repositoryMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }
}
