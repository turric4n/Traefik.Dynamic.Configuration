using System.Dynamic;
using YamlDotNet.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.UseYamlSerializerDeserializer();

var app = builder.Build();

app.MapGet("/", () =>
{
    var deserializer = app.Services.GetService<IDeserializer>();
    var serializer = app.Services.GetService<ISerializer>();
    var merged = GetMergedYml(deserializer, serializer);
    return merged;
});

app.Run();

string GetMergedYml(IDeserializer deserializer, ISerializer serializer)
{

    var currentDynamic = (dynamic)new ExpandoObject();
    currentDynamic.http = new Dictionary<dynamic, Dictionary<dynamic, dynamic>>();
    (currentDynamic.http as Dictionary<dynamic, Dictionary<dynamic, dynamic>>)?.Add("routers", new Dictionary<dynamic, dynamic>());
    (currentDynamic.http as Dictionary<dynamic, Dictionary<dynamic, dynamic>>)?.Add("services", new Dictionary<dynamic, dynamic>());

    var ymlFiles = Directory.GetFiles(@".", "*.yml");
    foreach (var file in ymlFiles)
    {
        try
        {
            var yamlContent = File.ReadAllText(file);
            var dynamicContent = (dynamic)deserializer.Deserialize<ExpandoObject>(yamlContent);

            if (dynamicContent.http != null)
            {
                foreach (var item in dynamicContent.http)
                {
                    if (item.Key == "routers")
                    {
                        foreach (var mainItem in currentDynamic.http)
                        {
                            if (mainItem.Key == "routers")
                            {
                                foreach (var subItem in item.Value)
                                {
                                    ((Dictionary<dynamic, dynamic>)mainItem.Value).Add(subItem.Key, subItem.Value);
                                }
                            }
                        }
                    }
                    else if (item.Key == "services")
                    {
                        foreach (var mainItem in currentDynamic.http)
                        {
                            if (mainItem.Key == "services")
                            {
                                foreach (var subItem in item.Value)
                                {
                                    ((Dictionary<dynamic, dynamic>)mainItem.Value).Add(subItem.Key, subItem.Value);
                                }
                            }
                        }

                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    var currentSerializedYml = serializer.Serialize(currentDynamic);

    return currentSerializedYml;
}