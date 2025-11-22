using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;

namespace StandardEoDEdition;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class StandardEodEditionExtension(
    ModHelper modHelper,
    LocaleService localeService,
    DatabaseServer databaseServer,
    ISptLogger<StandardEodEditionExtension> logger) : IOnLoad
{
    public Task OnLoad()
    {
        var serverLocale = localeService.GetDesiredServerLocale();
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var eodProfileCopy = DeepCopy(databaseServer.GetTables().Templates.Profiles["Edge Of Darkness"]);
        
        // var eodProfileCopy = databaseServer.GetTables().Templates.Profiles["Edge Of Darkness"] with
        // {
        //     
        // };

        if (eodProfileCopy.Bear?.Character?.Hideout is null 
            || eodProfileCopy.Usec?.Character?.Hideout is null)
        {
            logger.Error("The profile cannot be copied");
            return Task.CompletedTask;
        }
        
        eodProfileCopy.Bear.Character.Inventory = modHelper.GetJsonDataFromFile<BotBaseInventory>(
            pathToMod, Path.Combine("data", "bear_inventory.json"));
        eodProfileCopy.Usec.Character.Inventory = modHelper.GetJsonDataFromFile<BotBaseInventory>(
            pathToMod, Path.Combine("data", "usec_inventory.json"));
        
        eodProfileCopy.Bear.Character.Skills = modHelper.GetJsonDataFromFile<Skills>(
            pathToMod, Path.Combine("data", "skill_issue.json"));
        eodProfileCopy.Usec.Character.Skills = modHelper.GetJsonDataFromFile<Skills>(
            pathToMod, Path.Combine("data", "skill_issue.json"));
        
        eodProfileCopy.Bear.Character.Hideout.Areas = modHelper.GetJsonDataFromFile<List<BotHideoutArea>>(
            pathToMod, Path.Combine("data", "hideout_areas.json"));
        eodProfileCopy.Usec.Character.Hideout.Areas = modHelper.GetJsonDataFromFile<List<BotHideoutArea>>(
            pathToMod, Path.Combine("data", "hideout_areas.json"));
        
        eodProfileCopy.Bear.Character.Bonuses = modHelper.GetJsonDataFromFile<List<Bonus>>(
            pathToMod, Path.Combine("data", "bonuses.json"));
        eodProfileCopy.Usec.Character.Bonuses = modHelper.GetJsonDataFromFile<List<Bonus>>(
            pathToMod, Path.Combine("data", "bonuses.json"));
        
        eodProfileCopy.Bear.Trader = modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(
            pathToMod, Path.Combine("data", "traders.json"));
        eodProfileCopy.Usec.Trader = modHelper.GetJsonDataFromFile<ProfileTraderTemplate>(
            pathToMod, Path.Combine("data", "traders.json"));

        eodProfileCopy.DescriptionLocaleKey = serverLocale switch
        {
            "en" => "Standard edition with bonuses from Edge Of Darkness edition.",
            "ru" => "Стандартное издание с бонусами из Edge Of Darkness издания.",
            _ => ""
        };
        
        databaseServer.GetTables().Templates.Profiles["Standard EoD Edition"] = eodProfileCopy;

        return Task.CompletedTask;
    }
    
    private static T DeepCopy<T>(T obj)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new MongoIdConverter() }
        };
        
        var json = JsonSerializer.Serialize(obj, options);
        return JsonSerializer.Deserialize<T>(json, options)!;
    }
}