using DbServices;
using DbServices.Connection;
using DbServices.Domain;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Assert = NUnit.Framework.Assert;

#nullable disable

[TestFixture]
public class DbServiceTestContainerTests
{
    private PostgreSqlContainer _postgresContainer;
    private DbService _service;
    IAxataDataSource _dataSource;

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithPortBinding(5438, 5432)
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();

        _dataSource = new PostgresDataSource(_postgresContainer.GetConnectionString());

        _service = new DbService(_dataSource);
        await _service.MigrateAsync();
    }

    [SetUp]
    public async Task ClearBeforeEachTest()
    {
        using var cmd = _dataSource.DataSource.CreateCommand();

        cmd.CommandText = "DELETE FROM t_users;";
        await cmd.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task SaveAsync_Should_Insert_User()
    {
        var user = new User { Name = "Alice" };
        var id = await _service.SaveAsync(user);
        var fromDb = await _service.FindByIdAsync(id);

        Assert.IsNotNull(fromDb);
        Assert.That(fromDb.Name, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task DeleteAsync_Should_Remove_User()
    {
        var user = new User { Name = "Bob" };
        var id = await _service.SaveAsync(user);

        await _service.DeleteAsync(user);

        var deleted = await _service.FindByIdAsync(id);
        Assert.IsNull(deleted);
    }

    [Test]
    public async Task UpdateAsync_Should_Modify_User()
    {
        var user = new User { Name = "Charlie" };
        var id = await _service.SaveAsync(user);

        user.Id = id;
        user.Name = "Charlie Updated";

        await _service.UpdateAsync(user);

        var updated = await _service.FindByIdAsync(id);
        Assert.That(updated.Name, Is.EqualTo("Charlie Updated"));
    }

    [Test]
    public async Task GetAllUserAsync_Should_Return_Users()
    {
        await _service.SaveAsync(new User { Name = "User 1" });
        await _service.SaveAsync(new User { Name = "User 2" });

        var users = await _service.GetAllUserAsync();
        Assert.That(users.Count, Is.EqualTo(2));
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await _postgresContainer.DisposeAsync();
    }
}
