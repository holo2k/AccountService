namespace AccountService.Tests.Integration.Common;

[CollectionDefinition("SequentialIntegrationTests", DisableParallelization = true)]
public class SequentialIntegrationTestsCollection : ICollectionFixture<IntegrationTestFixture>
{
}