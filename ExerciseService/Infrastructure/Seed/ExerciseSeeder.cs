using ExcelDataReader;
using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace ExerciseService.Infrastructure.Seed;

public static class ExerciseSeeder
{
    public static async Task SeedAsync(ExerciseDbContext db, string filePath, bool forceReseed = false)
    {
        if (forceReseed)
        {
            await ClearAllDataAsync(db);
        }
        else if (await db.Exercises.AnyAsync())
        {
            return;
        }

        await SeedMainCategoriesAsync(db);
        await SeedTagsAsync(db);
        await ImportExercisesFromExcelAsync(db, filePath);
    }

    private static async Task SeedMainCategoriesAsync(ExerciseDbContext db)
    {
        var categories = new List<ExerciseMainCategory>
        {
            new() { Name = "all", DisplayName = "Всі вправи", FolderName = "all" },
            new() { Name = "whistling", DisplayName = "Свистячі", FolderName = "whistling" },
            new() { Name = "hushing", DisplayName = "Шиплячі", FolderName = "hushing" },
            new() { Name = "sound-l", DisplayName = "Звук Л", FolderName = "sound-l" },
            new() { Name = "sound-r", DisplayName = "Звук Р", FolderName = "sound-r" },
            new() { Name = "tongue-tip", DisplayName = "Кінчик язика", FolderName = "tongue-tip" }
        };

        db.ExerciseMainCategories.AddRange(categories);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTagsAsync(ExerciseDbContext db)
    {
        var tags = new List<ExerciseTag>();

        tags.AddRange(new[]
        {
            new ExerciseTag { Name = "type-static", Category = "type", DisplayName = "Статична" },
            new ExerciseTag { Name = "type-dynamic", Category = "type", DisplayName = "Динамічна" }
        });

        tags.AddRange(new[]
        {
            new ExerciseTag { Name = "organ-lips", Category = "organ", DisplayName = "Губи" },
            new ExerciseTag { Name = "organ-jaw", Category = "organ", DisplayName = "Нижня щелепа" },
            new ExerciseTag { Name = "organ-tongue", Category = "organ", DisplayName = "Язик" }
        });

        var sounds = new Dictionary<string, string>
        {
            ["sound-a"] = "Звук А",
            ["sound-b"] = "Звук Б",
            ["sound-v"] = "Звук В",
            ["sound-h"] = "Звук Г",
            ["sound-g"] = "Звук Ґ",
            ["sound-d"] = "Звук Д",
            ["sound-dzh"] = "Звук ДЖ",
            ["sound-dz"] = "Звук ДЗ",
            ["sound-e"] = "Звук Е",
            ["sound-zh"] = "Звук Ж",
            ["sound-z"] = "Звук З",
            ["sound-y"] = "Звук И",
            ["sound-i"] = "Звук І",
            ["sound-k"] = "Звук К",
            ["sound-l"] = "Звук Л",
            ["sound-m"] = "Звук М",
            ["sound-n"] = "Звук Н",
            ["sound-o"] = "Звук О",
            ["sound-p"] = "Звук П",
            ["sound-r"] = "Звук Р",
            ["sound-s"] = "Звук С",
            ["sound-t"] = "Звук Т",
            ["sound-u"] = "Звук У",
            ["sound-f"] = "Звук Ф",
            ["sound-kh"] = "Звук Х",
            ["sound-ts"] = "Звук Ц",
            ["sound-ch"] = "Звук Ч",
            ["sound-sh"] = "Звук Ш"
        };

        tags.AddRange(sounds.Select(kvp => new ExerciseTag
        {
            Name = kvp.Key,
            Category = "sound",
            DisplayName = kvp.Value
        }));

        db.ExerciseTags.AddRange(tags);
        await db.SaveChangesAsync();
    }

    private static async Task ImportExercisesFromExcelAsync(ExerciseDbContext db, string filePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var ds = reader.AsDataSet();

        if (ds.Tables.Count == 0)
        {
            throw new InvalidOperationException("Excel файл не містить жодної таблиці");
        }

        var mainCategories = await db.ExerciseMainCategories.ToDictionaryAsync(x => x.Name);
        var tags = await db.ExerciseTags.ToDictionaryAsync(x => x.Name);

        var exercises = new List<Exercise>();
        var exerciseTagMappings = new List<(string title, List<string> tagNames)>();

        foreach (DataTable table in ds.Tables)
        {
            if (table.Rows.Count <= 1)
            {
                continue;
            }

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var title = GetCellValue(row, 0);
                var category = GetCellValue(row, 1);
                var video = GetCellValue(row, 2);
                var type = GetCellValue(row, 3);
                var organ = GetCellValue(row, 4);
                var sounds = GetCellValue(row, 5);
                var description = GetCellValue(row, 6);

                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var mainCategoryName = GetMainCategoryName(category);
                if (!mainCategories.ContainsKey(mainCategoryName))
                {
                    mainCategoryName = "all";
                }

                var exercise = new Exercise
                {
                    Title = title,
                    Description = description,
                    VideoPath = BuildVideoPath(video, mainCategories[mainCategoryName].FolderName),
                    IconName = "exercise",
                    MainCategoryId = mainCategories[mainCategoryName].Id
                };

                exercises.Add(exercise);

                var exerciseTags = new List<string>();
                exerciseTags.AddRange(MapType(type));
                exerciseTags.AddRange(MapOrgan(organ));
                exerciseTags.AddRange(MapSounds(sounds));

                exerciseTagMappings.Add((title, exerciseTags.Distinct().ToList()));
            }
        }

        if (!exercises.Any())
        {
            return;
        }

        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        var exerciseIds = await db.Exercises
            .Where(e => exercises.Select(ex => ex.Title).Contains(e.Title))
            .ToDictionaryAsync(e => e.Title, e => e.Id);

        foreach (var (title, tagNames) in exerciseTagMappings)
        {
            if (!exerciseIds.ContainsKey(title))
                continue;

            var exerciseId = exerciseIds[title];

            foreach (var tagName in tagNames)
            {
                if (tags.ContainsKey(tagName))
                {
                    var tagId = tags[tagName].Id;

                    await db.Database.ExecuteSqlRawAsync(
                        "INSERT INTO ExerciseTagLinks (ExerciseId, TagId) VALUES ({0}, {1})",
                        exerciseId, tagId);
                }
            }
        }
    }

    private static string GetCellValue(DataRow row, int columnIndex)
    {
        if (columnIndex >= row.ItemArray.Length)
            return "";

        return row[columnIndex]?.ToString()?.Trim() ?? "";
    }

    private static string BuildVideoPath(string file, string folderName)
    {
        if (string.IsNullOrWhiteSpace(file))
            return "";

        file = file.Trim().ToLower().Replace(" ", "-");
        return $"/static/videos/{folderName}/{file}";
    }

    private static string GetMainCategoryName(string category)
    {
        var normalized = Normalize(category);

        var categoryMap = new Dictionary<string, string>
        {
            ["all"] = "all",
            ["whistling"] = "whistling",
            ["hushing"] = "hushing",
            ["sound-l"] = "sound-l",
            ["sound-r"] = "sound-r",
            ["tongue-tip"] = "tongue-tip",
        };

        if (categoryMap.ContainsKey(normalized))
        {
            return categoryMap[normalized];
        }

        foreach (var kvp in categoryMap)
        {
            if (normalized.Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }

        return "all";
    }

    private static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return "";

        return s
            .ToLower()
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("'", "'")
            .Replace("ʼ", "'")
            .Trim();
    }

    private static List<string> MapType(string raw)
    {
        raw = Normalize(raw);
        var list = new List<string>();

        if (raw.Contains("статична"))
            list.Add("type-static");

        if (raw.Contains("динамічна"))
            list.Add("type-dynamic");

        return list;
    }

    private static readonly Dictionary<string, string> OrganMap = new()
    {
        ["губи"] = "organ-lips",
        ["нижня щелепа"] = "organ-jaw",
        ["язик"] = "organ-tongue"
    };

    private static List<string> MapOrgan(string raw)
    {
        raw = Normalize(raw);

        if (string.IsNullOrEmpty(raw))
            return new List<string>();

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Normalize(x))
            .Where(x => OrganMap.ContainsKey(x))
            .Select(x => OrganMap[x])
            .ToList();
    }

    private static readonly Dictionary<string, string> SoundMap = new()
    {
        ["а"] = "sound-a",
        ["б"] = "sound-b",
        ["в"] = "sound-v",
        ["г"] = "sound-h",
        ["ґ"] = "sound-g",
        ["д"] = "sound-d",
        ["дж"] = "sound-dzh",
        ["дз"] = "sound-dz",
        ["е"] = "sound-e",
        ["ж"] = "sound-zh",
        ["з"] = "sound-z",
        ["и"] = "sound-y",
        ["і"] = "sound-i",
        ["к"] = "sound-k",
        ["л"] = "sound-l",
        ["м"] = "sound-m",
        ["н"] = "sound-n",
        ["о"] = "sound-o",
        ["п"] = "sound-p",
        ["р"] = "sound-r",
        ["с"] = "sound-s",
        ["т"] = "sound-t",
        ["у"] = "sound-u",
        ["ф"] = "sound-f",
        ["х"] = "sound-kh",
        ["ц"] = "sound-ts",
        ["ч"] = "sound-ch",
        ["ш"] = "sound-sh"
    };

    private static List<string> MapSounds(string raw)
    {
        raw = Normalize(raw);

        if (string.IsNullOrEmpty(raw))
            return new List<string>();

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Normalize(x))
            .Where(x => SoundMap.ContainsKey(x))
            .Select(x => SoundMap[x])
            .ToList();
    }

    private static async Task ClearAllDataAsync(ExerciseDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ExerciseTagLinks");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM Exercises");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ExerciseTags");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM ExerciseMainCategories");

        var tables = new[] {
            "Exercises", "ExerciseTags", "ExerciseMainCategories"
        };

        foreach (var table in tables)
        {
            await db.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{table}', RESEED, 0)");
        }
    }
}