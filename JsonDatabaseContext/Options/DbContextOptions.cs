namespace JsonDatabaseContext.Options;

public class DbContextOptions
{
    public static string SectionKey = nameof(DbContextOptions);

    public string BasePath { get; set; }
}
