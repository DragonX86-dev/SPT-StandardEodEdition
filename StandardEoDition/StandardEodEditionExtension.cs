using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using Path = System.IO.Path;

namespace StandardEoDition;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class StandardEodEditionExtension(
    ModHelper modHelper,
    DatabaseServer databaseServer,
    ISptLogger<StandardEodEditionExtension> logger) : IOnLoad
{
    public Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var eodProfileDeepCopy = DeepCopy(databaseServer.GetTables().Templates.Profiles["Edge Of Darkness"]);

        if (eodProfileDeepCopy.Bear?.Character?.Hideout is null 
            || eodProfileDeepCopy.Usec?.Character?.Hideout is null)
        {
            logger.Error("The profile cannot be copied");
            return Task.CompletedTask;
        }
        
        eodProfileDeepCopy.Bear.Character.Inventory = modHelper.GetJsonDataFromFile<BotBaseInventory>(
            pathToMod, Path.Combine("data", "bear_inventory.json"));
        eodProfileDeepCopy.Usec.Character.Inventory = modHelper.GetJsonDataFromFile<BotBaseInventory>(
            pathToMod, Path.Combine("data", "usec_inventory.json"));
        
        eodProfileDeepCopy.Bear.Character.Skills = modHelper.GetJsonDataFromFile<Skills>(
            pathToMod, Path.Combine("data", "skill_issue.json"));
        eodProfileDeepCopy.Usec.Character.Skills = modHelper.GetJsonDataFromFile<Skills>(
            pathToMod, Path.Combine("data", "skill_issue.json"));
        
        eodProfileDeepCopy.Bear.Character.Hideout.Areas = modHelper.GetJsonDataFromFile<List<BotHideoutArea>>(
            pathToMod, Path.Combine("data", "hideout_areas.json"));
        eodProfileDeepCopy.Usec.Character.Hideout.Areas = modHelper.GetJsonDataFromFile<List<BotHideoutArea>>(
            pathToMod, Path.Combine("data", "hideout_areas.json"));
        
        eodProfileDeepCopy.Bear.Character.Bonuses = modHelper.GetJsonDataFromFile<List<Bonus>>(
            pathToMod, Path.Combine("data", "bonuses.json"));
        eodProfileDeepCopy.Usec.Character.Bonuses = modHelper.GetJsonDataFromFile<List<Bonus>>(
            pathToMod, Path.Combine("data", "bonuses.json"));
        
        eodProfileDeepCopy.Bear.Trader = modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(
            pathToMod, Path.Combine("data", "traders.json"));
        eodProfileDeepCopy.Usec.Trader = modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(
            pathToMod, Path.Combine("data", "traders.json"));

        eodProfileDeepCopy.DescriptionLocaleKey = "Standard edition with EoD chips";

        databaseServer.GetTables().Templates.Profiles["Standard Eod Edition"] = eodProfileDeepCopy;

        return Task.CompletedTask;
    }

    private static T DeepCopy<T>(T obj)
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj))!;
    }
}