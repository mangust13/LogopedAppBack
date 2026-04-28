using ExcelDataReader;
using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace ExerciseService.Infrastructure.Seed;

public static class ExerciseSeeder
{
    public static async Task SeedAsync(ExerciseDbContext db, string filePath)
    {
        await SeedDefaultComplexesAsync(db);
        await SeedTagsAsync(db);
        await ImportExercisesFromExcelAsync(db, filePath);
    }

    private static async Task SeedDefaultComplexesAsync(ExerciseDbContext db)
    {
        if (await db.Complexes.AnyAsync()) return;

        var complexes = new List<Complex>
        {
            new() { Name = "all", DisplayName = "Всі вправи", FolderName = "all", IsDefault = true, Description = "" },
            new() { Name = "whistling", DisplayName = "Свистячі", FolderName = "whistling", IsDefault = true, Description = "Комплекс вправ для свистячих звуків" },
            new() { Name = "hushing", DisplayName = "Шиплячі", FolderName = "hushing", IsDefault = true, Description = "Комплекс вправ для шиплячих звуків" },
            new() { Name = "sound-l", DisplayName = "Звук Л", FolderName = "sound-l", IsDefault = true, Description = "Комплекс вправ для звука Л" },
            new() { Name = "sound-r", DisplayName = "Звук Р", FolderName = "sound-r", IsDefault = true, Description = "Комплекс вправ для звука Р" },
            new() { Name = "tongue-tip", DisplayName = "Кінчик язика", FolderName = "tongue-tip", IsDefault = true, Description = "Комплекс вправ для кінчика язика" }
        };

        db.Complexes.AddRange(complexes);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTagsAsync(ExerciseDbContext db)
    {
        if (await db.ExerciseTags.AnyAsync()) return;

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
        if (await db.Exercises.AnyAsync()) return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var ds = reader.AsDataSet();

        if (ds.Tables.Count == 0)
        {
            throw new InvalidOperationException("Excel файл не містить жодної таблиці");
        }

        var complexes = await db.Complexes.Where(c => c.IsDefault).ToDictionaryAsync(c => c.Name
    );
        var tags = await db.ExerciseTags.ToDictionaryAsync(x => x.Name);
        var exercises = new List<Exercise>();
        var exerciseTagMappings = new List<(string title, List<string> tagNames)>();
        var complexItemMappings = new List<(string title, string complexName)>();

        // Dictionary to map sheet names to complex names
        var sheetToComplexMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["all"] = "all",
            ["всі вправи"] = "all",
            ["шиплячі"] = "hushing",
            ["свистячі"] = "whistling",
            ["звук л"] = "sound-l",
            ["звук р"] = "sound-r",
            ["кінчик язика"] = "tongue-tip"
        };

        foreach (DataTable table in ds.Tables)
        {
            if (table.Rows.Count <= 1)
            {
                continue;
            }

            // Determine the complex for this sheet
            string sheetComplexName = "all"; // Default
            string sheetName = table.TableName.Trim().ToLower();

            foreach (var kvp in sheetToComplexMap)
            {
                if (sheetName.Contains(kvp.Key))
                {
                    sheetComplexName = kvp.Value;
                    break;
                }
            }

            for (int i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var title = GetCellValue(row, 0);
                var complexName = GetCellValue(row, 1); // This is still read but not used for mapping
                var video = GetCellValue(row, 2);
                var type = GetCellValue(row, 3);
                var organ = GetCellValue(row, 4);
                var sounds = GetCellValue(row, 5);
                var description = GetCellValue(row, 6);

                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                // Use the sheet's complex name instead of the cell value
                var normalizedComplexName = sheetComplexName;

                var exercise = new Exercise
                {
                    Title = title,
                    Description = description,
                    VideoPath = BuildVideoPath(video, normalizedComplexName),
                    IconName = "exercise"
                };

                exercises.Add(exercise);

                var exerciseTags = new List<string>();
                exerciseTags.AddRange(MapType(type));
                exerciseTags.AddRange(MapOrgan(organ));
                exerciseTags.AddRange(MapSounds(sounds));

                exerciseTagMappings.Add((title, exerciseTags.Distinct().ToList()));

                // Add the exercise to the complex based on the sheet name
                complexItemMappings.Add((title, normalizedComplexName));
            }
        }

        // Rest of the method remains the same...
        if (!exercises.Any())
        {
            return;
        }

        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        var exerciseIds = await db.Exercises
            .Where(e => exercises.Select(ex => ex.Title).Contains(e.Title))
            .ToDictionaryAsync(e => e.Title, e => e.Id);

        // Add tags
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

        // Add exercises to complexes
        var complexItemOrder = new Dictionary<string, int>();
        foreach (var complexName in complexes.Keys)
        {
            complexItemOrder[complexName] = 1;
        }

        foreach (var (title, complexName) in complexItemMappings)
        {
            if (!exerciseIds.ContainsKey(title) || !complexes.ContainsKey(complexName))
                continue;

            var exerciseId = exerciseIds[title];
            var complexId = complexes[complexName].Id;

            db.ComplexItems.Add(new ComplexItem
            {
                ComplexId = complexId,
                ExerciseId = exerciseId,
                Order = complexItemOrder[complexName]++
            });
        }

        await db.SaveChangesAsync();
    }

    private static string GetComplexName(string complexFromExcel)
    {
        var normalized = Normalize(complexFromExcel);

        var categoryMap = new Dictionary<string, string>
        {
            ["всі вправи"] = "all",
            ["свистячі"] = "whistling",
            ["шиплячі"] = "hushing",
            ["звук л"] = "sound-l",
            ["звук р"] = "sound-r",
            ["кінчик язика"] = "tongue-tip",
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
}