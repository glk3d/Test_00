namespace TestAutomateFunction;

using Speckle.Automate.Sdk;
using Speckle.Automate.Sdk.Schema;
using Speckle.Automate.Sdk.Test;
using Speckle.Core.Api;
using Speckle.Core.Credentials;


/*
  cd38d517485dfce646e20af2ac69486284b1eb7cd6
  af744e71e84e9a9c69d8f6edc57ee15288933ef965
*/

[TestFixture]
public sealed class AutomationContextTest : IDisposable
{
    private Client client;
    private Account account;

    [OneTimeSetUp]
    public void Setup()
    {
        var token = TestAutomateEnvironment.GetSpeckleToken();

        var url = TestAutomateEnvironment.GetSpeckleServerUrl().ToString();
        var serverInfo = new ServerInfo { url = url };

        account = new Account
        {
            token = token,
            serverInfo = serverInfo
        };

        client = new Client(account);
    }

    [Test]
    public async Task TestFunctionRun()
    {
        var inputs = new FunctionInputs
        {
            SpeckleTypeToCount = "Base",
            SpeckleTypeTargetCount = 1,
            ExternalServiceKey = "FÜNF",
        };

        /*
        var automationRunData = new AutomationRunData()
        {
            ProjectId = TestAutomateEnvironment.GetSpeckleProjectId(),
            AutomationId = TestAutomateEnvironment.GetSpeckleAutomationId(),
            SpeckleServerUrl = TestAutomateEnvironment.GetSpeckleServerUrl().ToString(),
        };
        */
        var automationRunData = await TestAutomateUtils.CreateTestRun(client);


        var automationContext = await AutomationRunner.RunFunction(
          AutomateFunction.Run,
         automationRunData,
          account.token,
          inputs
        );

        Assert.That(automationContext.RunStatus, Is.EqualTo("SUCCEEDED"));
    }

    public void Dispose()
    {
        client.Dispose();
        TestAutomateEnvironment.Clear();
    }
}
