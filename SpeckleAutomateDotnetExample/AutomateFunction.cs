using Objects;
using Speckle.Automate.Sdk;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;
using SpeckleStructuralModel = Objects.Structural.Analysis.Model;
using Speckle.Core.Api;
using Speckle.Core.Transports;
using Objects.Structural.Geometry;

public static class AutomateFunction
{
    public static async Task Run(AutomationContext automationContext, FunctionInputs functionInputs)
    {
        var breakpoint = "";

        _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

        Console.WriteLine("Starting execution");
        Console.WriteLine("-------------------------------------------------------------");

        // get newly pushed model
        var commitData = await automationContext.ReceiveVersion();

        // receive lca data from another stream
        var lcaData = await GetLCAValues(automationContext, functionInputs);

        // maybe sanity check
        // 
        int elemCount = 0;
        int materialCount = 0;

        var overallStageABC = 0.0;
        var overallStageD = 0.0;
        foreach (var model in FilterStructuralModel(commitData))
        {
            if (model is null || model.elements is null)
                continue;

            // get elements grouped by material
            var materialGroups = model.elements
                .GroupBy(e =>
                e is Element1D e1d ? e1d.property.material.name :
                e is Element2D e2d ? e2d.property.material.name :
                null)
                .Where(g => g.Key != null);

            // calculate lca and attach to elements
            foreach (var group in materialGroups)
            {
                materialCount++;

                var lca = lcaData
                    .FirstOrDefault(i => i.Type == group.Key);
                foreach (var element in group)
                {
                    elemCount++;
                }
            }
        }

        // add another commit
        // await automationContext.CreateNewVersionInProject(commitObject, "Another Model", "Commit Message");

        Console.WriteLine("-------------------------------------------------------------");
        automationContext.MarkRunSuccess($"Different materials: {materialCount} for {elemCount} Elements.");
        Console.WriteLine("-------------------------------------------------------------");
    }

    private static List<SpeckleStructuralModel> FilterStructuralModel(Base @base)
    {
        var traverse = DefaultTraversal.CreateTraversalFunc();
        return traverse.Traverse(@base)
            .Select(o => o.Current)
            .OfType<SpeckleStructuralModel>()
            .ToList();
    }

    private static async Task<List<LCAValue>> GetLCAValues(AutomationContext automationContext, FunctionInputs functionInputs)
    {
        // find latest excel lca
        var streams = await automationContext.SpeckleClient.StreamGetBranches("b613e0ba83");
        var commits = streams.Where(s => s.name == "excel").Select(s => s.commits).ToList();
        var latestObjectId = commits[0].items[0].referencedObject;

        // receive excel lca
        var transport = new ServerTransport(automationContext.SpeckleClient.Account, "b613e0ba83");
        var readExcelData = await Operations.Receive(latestObjectId, transport);

        var data = readExcelData["data"] as List<object>;
        data.RemoveAt(0); // Remove excel header

        var lcaValuesList = new List<LCAValue>();
        foreach (List<object> item in data)
        {
            lcaValuesList.Add(
                new LCAValue()
                {
                    Material = Convert.ToString(item[0]),
                    Type = item[1] as string,
                    StageAbC = Convert.ToDouble(item[3]),
                    StageD = Convert.ToDouble(item[3]),
                    Unit = item[4] as string,
                });
        }

        return lcaValuesList;
    }

    private struct LCAValue
    {
        public string Material { get; set; }

        public string Type { get; set; }

        public double StageAbC { get; set; }

        public double StageD { get; set; }

        public string Unit { get; set; }
    }
}
