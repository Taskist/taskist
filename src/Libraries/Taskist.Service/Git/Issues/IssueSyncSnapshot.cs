using Newtonsoft.Json;

namespace Taskist.Service.Git.Issues;

public class IssueSyncSnapshot
{
    public Dictionary<string, IssueSyncField> Fields { get; set; } = new();

    public static IssueSyncSnapshot Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new IssueSyncSnapshot();

        try
        {
            return JsonConvert.DeserializeObject<IssueSyncSnapshot>(json) ?? new IssueSyncSnapshot();
        }
        catch (JsonException)
        {
            return new IssueSyncSnapshot();
        }
    }

    public string Serialize() => JsonConvert.SerializeObject(this);
}
