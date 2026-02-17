using System.ComponentModel;
using System.Reflection;

namespace InventoryPro.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    public static T? GetEnumFromDescription<T>(string description) where T : struct, Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute?.Description == description)
                return (T?)field.GetValue(null);

            if (field.Name == description)
                return (T?)field.GetValue(null);
        }

        return null;
    }

    public static IEnumerable<T> GetValues<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>();
    }

    public static Dictionary<int, string> ToDictionary<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .ToDictionary(e => Convert.ToInt32(e), e => e.GetDescription());
    }
}
