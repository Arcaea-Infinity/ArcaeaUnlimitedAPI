using Newtonsoft.Json;

#pragma warning disable CS8618

namespace ArcaeaUnlimitedAPI.Json.Packlist;

public class NameLocalized
{
    [JsonProperty("en")]
    public string En { get; set; }
}

public class PackItem
{
    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("pack_parent")]
    public string PackParent { get; set; } = string.Empty;

    [JsonProperty("name_localized")]
    public NameLocalized NameLocalized { get; set; }
}

public class Packlist
{
    [JsonProperty("packs")]
    public List<PackItem> Packs { get; set; }
}
