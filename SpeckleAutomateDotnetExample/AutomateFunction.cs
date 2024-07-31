using Objects;
using Speckle.Automate.Sdk;
using Speckle.Core.Models.Extensions;

public static class AutomateFunction
{
    public static async Task Run(AutomationContext automationContext, FunctionInputs functionInputs)
    {
        _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

        Console.WriteLine("Starting execution");

        Console.WriteLine("Receiving version");
        var commitObject = await automationContext.ReceiveVersion();
        Console.WriteLine("Received version: " + commitObject);

        await automationContext.CreateNewVersionInProject(commitObject, "Another Model", "Fünf");

        if (commitObject.applicationId.Contains("Karamba3D"))
        {
            automationContext.MarkRunSuccess($"Commit is from Karamba3D Model.");
        }

        Console.WriteLine(commitObject.TryGetName());

        var count = commitObject
          .Flatten()
          .Count(b => b.speckle_type == functionInputs.SpeckleTypeToCount);

        Console.WriteLine($"Counted {count} objects");

        /*
        if (count < functionInputs.SpeckleTypeTargetCount)
        {
          automationContext.MarkRunFailed(
            $"Counted {count} objects where {functionInputs.SpeckleTypeTargetCount} were expected"
          );
          return;
        }
        */

        automationContext.MarkRunSuccess($"Counted {count} objects");
    }
}
