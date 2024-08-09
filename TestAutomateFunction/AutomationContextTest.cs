namespace TestAutomateFunction;

using Speckle.Automate.Sdk;
using Speckle.Automate.Sdk.Schema;
using Speckle.Automate.Sdk.Test;
using Speckle.Core.Api;
using Speckle.Core.Credentials;

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
            SpeckleTypeToCount = "Objects.Structural.Geometry.Element1D",
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
