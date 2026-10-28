namespace Commons.Domain;

public static class TypeExtensions
{
    public static string FriendlyName(this Type type)
    {
        if (type.IsGenericType)
        {
            var namePrefix = type.Name.Split('`', StringSplitOptions.RemoveEmptyEntries)[0];
            var genericParameters = type.GetGenericArguments().Select(FriendlyName).ToCsv();
            return namePrefix + "<" + genericParameters + ">";
        }

        return type.Name;
    }

    private static string ToCsv(this IEnumerable<object> collectionToConvert, string separator = ", ")
    {
        return string.Join(separator, collectionToConvert.Select(o => o.ToString()));
    }
}
