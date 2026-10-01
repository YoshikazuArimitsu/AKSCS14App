using CS14App.Api.Services;

namespace CS14App.Api.Tests.Services;

public class MessageRepositoryTests
{
    [Fact]
    public void IsConfigured_WithoutDataSource_ReturnsFalse()
    {
        var repository = new MessageRepository(dataSource: null);

        Assert.False(repository.IsConfigured);
    }

    [Fact]
    public async Task GetMessagesAsync_WithoutDataSource_Throws()
    {
        var repository = new MessageRepository(dataSource: null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.GetMessagesAsync(limit: 100, offset: 0, CancellationToken.None));
    }
}
