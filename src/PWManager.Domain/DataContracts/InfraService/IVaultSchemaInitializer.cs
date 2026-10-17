namespace PWManager.Domain.DataContracts.InfraService;

public interface IVaultSchemaInitializer
{
    Task EnsureAsync();
}
