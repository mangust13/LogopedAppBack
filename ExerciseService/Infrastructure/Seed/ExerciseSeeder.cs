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
            new ExerciseTag { Name = "organ-jaw", Category = "organ", DisplayName = "Нижня щелепа" }
        });

        var sounds = new Dictionary<string, string>
        {
            ["sound-a"] = "Звук А",
            ["sound-e"] = "Звук Е",
            ["sound-y"] = "Звук И",
            ["sound-i"] = "Звук І",
            ["sound-o"] = "Звук О",
            ["sound-u"] = "Звук У",
            ["sound-s"] = "Звук С",
            ["sound-z"] = "Звук З",
            ["sound-ts"] = "Звук Ц",
            ["sound-dz"] = "Звук ДЗ",
            ["sound-sh"] = "Звук Ш",
            ["sound-zh"] = "Звук Ж",
            ["sound-ch"] = "Звук Ч",
            ["sound-dzh"] = "Звук ДЖ"
        };

        tags.AddRange(sounds.Select(kvp => new ExerciseTag
        {
            Name = kvp.Key,
            Category = "sound",
            DisplayName = kvp.Value
        }));

        var muscles = new Dictionary<string, string>
        {
            ["muscle-digastric"] = "Двочеревцевий м'яз",
            ["muscle-mylohyoid"] = "Щелепно-під'язиковий м'яз",
            ["muscle-geniohyoid"] = "Підборідно-під'язиковий м'яз",
            ["muscle-lateral-pterygoid"] = "Латеральний крилоподібний м'яз",
            ["muscle-orbicularis-oris"] = "Круговий м'яз рота",
            ["muscle-zygomaticus-major"] = "Великий виличний м'яз",
            ["muscle-zygomaticus-minor"] = "Малий виличний м'яз",
            ["muscle-buccinator"] = "Щічний м'яз",
            ["muscle-masseter"] = "Жувальний м'яз",
            ["muscle-temporalis"] = "Скроневий м'яз",
            ["muscle-medial-pterygoid"] = "Медіальний крилоподібний м'яз",
            ["muscle-risorius"] = "М'яз сміху",
            ["muscle-mentalis"] = "Підборідний м'яз",
            ["muscle-levator-labii-superioris"] = "Підіймач верхньої губи",
            ["muscle-levator-labii-superioris-alaeque-nasi"] = "Підіймач верхньої губи і крила носа",
            ["muscle-depressor-labii-inferioris"] = "Опускач нижньої губи"
        };

        tags.AddRange(muscles.Select(kvp => new ExerciseTag
        {
            Name = kvp.Key,
            Category = "muscle",
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

        var table = ds.Tables[0];

        if (table.Rows.Count <= 1)
        {
            return;
        }

        var mainCategories = await db.ExerciseMainCategories.ToDictionaryAsync(x => x.Name);
        var tags = await db.ExerciseTags.ToDictionaryAsync(x => x.Name);

        var exercises = new List<Exercise>();
        var exerciseTagMappings = new List<(string title, List<string> tagNames)>();

        for (int i = 1; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];

            var title = GetCellValue(row, 0);
            var category = GetCellValue(row, 1);
            var video = GetCellValue(row, 2);
            var type = GetCellValue(row, 3);
            var organ = GetCellValue(row, 4);
            var sounds = GetCellValue(row, 5);
            var muscles = GetCellValue(row, 6);
            var description = GetCellValue(row, 7);

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
            exerciseTags.AddRange(MapMuscles(muscles));

            exerciseTagMappings.Add((title, exerciseTags.Distinct().ToList()));
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

            foreach
 (var tagName in tagNames)
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
            ["свистячі"] = "whistling",
            ["шиплячі"] = "hushing",
            ["сонорні"] = "sound-l",
            ["всі"] = "all",
            ["whistling"] = "whistling",
            ["hushing"] = "hushing",
            ["sound-l"] = "sound-l",
            ["sound-r"] = "sound-r",
            ["tongue-tip"] = "tongue-tip",
            ["all"] = "all"
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
        ["нижня щелепа"] = "organ-jaw"
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
        ["е"] = "sound-e",
        ["и"] = "sound-y",
        ["і"] = "sound-i",
        ["о"] = "sound-o",
        ["у"] = "sound-u",
        ["с"] = "sound-s",
        ["з"] = "sound-z",
        ["ц"] = "sound-ts",
        ["дз"] = "sound-dz",
        ["ш"] = "sound-sh",
        ["ж"] = "sound-zh",
        ["ч"] = "sound-ch",
        ["дж"] = "sound-dzh"
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

    private static readonly Dictionary<string, string> MuscleMap = new()
    {
        ["двочеревцевий м'яз"] = "muscle-digastric",
        ["щелепно-під'язиковий м'яз"] = "muscle-mylohyoid",
        ["підборідно-під'язиковий м'яз"] = "muscle-geniohyoid",
        ["латеральний крилоподібний м'яз"] = "muscle-lateral-pterygoid",
        ["круговий м'яз рота"] = "muscle-orbicularis-oris",
        ["великий виличний м'яз"] = "muscle-zygomaticus-major",
        ["малий величний м'яз"] = "muscle-zygomaticus-minor",
        ["щічний м'яз"] = "muscle-buccinator",
        ["жувальний м'яз"] = "muscle-masseter",
        ["скроневий м'яз"] = "muscle-temporalis",
        ["медіальний крилоподібний м'яз"] = "muscle-medial-pterygoid",
        ["м'яз сміху"] = "muscle-risorius",
        ["підборідний м'яз"] = "muscle-mentalis",
        ["підіймач верхньої губи"] = "muscle-levator-labii-superioris",
        ["підіймач верхньої губи і крила носа"] = "muscle-levator-labii-superioris-alaeque-nasi",
        ["опускач нижньої губи"] = "muscle-depressor-labii-inferioris"
    };

    private static List<string> MapMuscles(string raw)
    {
        raw = Normalize(raw);

        if (string.IsNullOrEmpty(raw))
            return new List<string>();

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => Normalize(x))
            .Where(x => MuscleMap.ContainsKey(x))
            .Select(x => MuscleMap[x])
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
            try
            {
                await db.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('{table}', RESEED, 0)");
            }
            catch
            {
            }
        }
    }
}