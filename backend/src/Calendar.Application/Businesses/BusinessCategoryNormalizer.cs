namespace Calendar.Application.Businesses;

internal static class BusinessCategoryNormalizer
{
    public static string? Normalize(string? value)
    {
        return value switch
        {
            "Barberia" => "barber",
            "Estetica" => "beauty",
            "Fisioterapia" => "physiotherapy",
            "Clases" => "classes",
            "Consultas" => "consulting",
            _ => value
        };
    }
}
