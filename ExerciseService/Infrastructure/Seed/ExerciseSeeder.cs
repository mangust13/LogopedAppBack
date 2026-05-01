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
        if (await db.Complexes.AnyAsync(c => c.IsDefault))
            return;

        var complexes = new List<Complex>
        {
            new()
            {
                Name = "all",
                DisplayName = "Всі вправи",
                FolderName = "all",
                IsDefault = true,
                IsActive = true,
                Description = ""
            },
            new()
            {
                Name = "whistling",
                DisplayName = "Свистячі",
                FolderName = "whistling",
                IsDefault = true,
                IsActive = true,
                Description = "Комплекс вправ для свистячих звуків"
            },
            new()
            {
                Name = "hushing",
                DisplayName = "Шиплячі",
                FolderName = "hushing",
                IsDefault = true,
                IsActive = true,
                Description = "Комплекс вправ для шиплячих звуків"
            },
            new()
            {
                Name = "sound-l",
                DisplayName = "Звук Л",
                FolderName = "sound-l",
                IsDefault = true,
                IsActive = true,
                Description = "Комплекс вправ для звука Л"
            },
            new()
            {
                Name = "sound-r",
                DisplayName = "Звук Р",
                FolderName = "sound-r",
                IsDefault = true,
                IsActive = true,
                Description = "Комплекс вправ для звука Р"
            },
            new()
            {
                Name = "tongue-tip",
                DisplayName = "Кінчик язика",
                FolderName = "tongue-tip",
                IsDefault = true,
                IsActive = true,
                Description = "Комплекс вправ для кінчика язика"
            }
        };

        db.Complexes.AddRange(complexes);
        await db.SaveChangesAsync();
    }

    private static async Task SeedTagsAsync(ExerciseDbContext db)
    {
        if (await db.ExerciseTags.AnyAsync())
            return;

        var tags = new List<ExerciseTag>
        {
            new() { Name = "type-static",   Category = "type",   DisplayName = "Статична" },
            new() { Name = "type-dynamic",  Category = "type",   DisplayName = "Динамічна" },
            new() { Name = "organ-lips",    Category = "organ",  DisplayName = "Губи" },
            new() { Name = "organ-jaw",     Category = "organ",  DisplayName = "Нижня щелепа" },
            new() { Name = "organ-tongue",  Category = "organ",  DisplayName = "Язик" },
        };

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
            ["sound-sh"] = "Звук Ш",
        };

        tags.AddRange(sounds.Select(s => new ExerciseTag
        {
            Name = s.Key,
            Category = "sound",
            DisplayName = s.Value,
        }));

        db.ExerciseTags.AddRange(tags);
        await db.SaveChangesAsync();
    }

    private static async Task ImportExercisesFromExcelAsync(ExerciseDbContext db, string filePath)
    {
        if (await db.Exercises.AnyAsync())
            return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet();

        if (dataSet.Tables.Count == 0)
            throw new InvalidOperationException("Excel файл не містить жодної таблиці");

        var tags = await db.ExerciseTags.ToDictionaryAsync(x => x.Name);
        var complexes = await db.Complexes
            .Where(c => c.IsDefault)
            .ToDictionaryAsync(c => c.Name);

        var exercises = new List<Exercise>();
        var tagMappings = new List<(Exercise Exercise, List<string> TagNames)>();
        var complexMappings = new List<(Exercise Exercise, string ComplexName)>();

        foreach (DataTable table in dataSet.Tables)
        {
            if (table.Rows.Count <= 1)
                continue;

            var complexName = GetComplexName(table.TableName);
            var folderName = complexes.TryGetValue(complexName, out var complex)
                ? complex.FolderName
                : "all";

            for (var i = 1; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var title = GetCell(row, 0);
                var videoFile = GetCell(row, 2);
                var type = GetCell(row, 4);
                var organ = GetCell(row, 5);
                var sounds = GetCell(row, 6);
                var description = GetCell(row, 7);

                if (string.IsNullOrWhiteSpace(title))
                    continue;

                var fileName = NormalizeFileName(videoFile);

                var exercise = new Exercise
                {
                    Title = title,
                    Description = description,
                    VideoPath = BuildPath("videos", folderName, fileName, ".mp4"),
                    ImagePath = BuildPath("images", folderName, fileName, ".jpg"),
                };

                exercises.Add(exercise);

                var exerciseTags = new List<string>();
                exerciseTags.AddRange(MapType(type));
                exerciseTags.AddRange(MapOrgan(organ));
                exerciseTags.AddRange(MapSounds(sounds));

                tagMappings.Add((exercise, exerciseTags.Distinct().ToList()));
                complexMappings.Add((exercise, complexName));
            }
        }

        if (!exercises.Any())
            return;

        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        var tagLinks = new List<ExerciseTagLink>();
        foreach (var (exercise, tagNames) in tagMappings)
        {
            foreach (var tagName in tagNames)
            {
                if (!tags.TryGetValue(tagName, out var tag))
                    continue;

                tagLinks.Add(new ExerciseTagLink
                {
                    ExerciseId = exercise.Id,
                    TagId = tag.Id,
                });
            }
        }

        if (tagLinks.Any())
            db.ExerciseTagLinks.AddRange(tagLinks);

        var orderMap = complexes.Keys.ToDictionary(n => n, _ => 1);
        var complexItems = new List<ComplexItem>();

        foreach (var (exercise, complexName) in complexMappings)
        {
            if (!complexes.TryGetValue(complexName, out var c))
                continue;

            complexItems.Add(new ComplexItem
            {
                ComplexId = c.Id,
                ExerciseId = exercise.Id,
                Order = orderMap[complexName]++,
            });
        }

        if (complexItems.Any())
            db.ComplexItems.AddRange(complexItems);

        await db.SaveChangesAsync();
    }

    private static string BuildPath(string type, string folder, string fileName, string ext)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "";

        return $"/static/preparation/{type}/{folder}/{fileName}{ext}";
    }

    private static string NormalizeFileName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";

        var withoutExt = Path.GetFileNameWithoutExtension(raw.Trim());
        return withoutExt.ToLower().Replace(" ", "-");
    }

    private static string GetComplexName(string sheetName)
    {
        var normalized = Normalize(sheetName);

        var map = new Dictionary<string, string>
        {
            ["all"] = "all",
            ["всі вправи"] = "all",
            ["свистячі"] = "whistling",
            ["шиплячі"] = "hushing",
            ["звук л"] = "sound-l",
            ["звук р"] = "sound-r",
            ["кінчик язика"] = "tongue-tip",
        };

        foreach (var item in map)
        {
            if (normalized.Contains(item.Key))
                return item.Value;
        }

        return "all";
    }

    private static string GetCell(DataRow row, int col)
    {
        if (col >= row.ItemArray.Length)
            return "";

        return row[col]?.ToString()?.Trim() ?? "";
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value
            .ToLower()
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("ʼ", "'")
            .Trim();
    }

    private static List<string> MapType(string raw)
    {
        raw = Normalize(raw);
        var result = new List<string>();

        if (raw.Contains("статична")) result.Add("type-static");
        if (raw.Contains("динамічна")) result.Add("type-dynamic");

        return result;
    }

    private static readonly Dictionary<string, string> OrganMap = new()
    {
        ["губи"] = "organ-lips",
        ["нижня щелепа"] = "organ-jaw",
        ["язик"] = "organ-tongue",
    };

    private static List<string> MapOrgan(string raw)
    {
        raw = Normalize(raw);

        if (string.IsNullOrEmpty(raw))
            return new List<string>();

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
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
        ["ш"] = "sound-sh",
    };

    private static List<string> MapSounds(string raw)
    {
        raw = Normalize(raw);

        if (string.IsNullOrEmpty(raw))
            return new List<string>();

        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .Where(x => SoundMap.ContainsKey(x))
            .Select(x => SoundMap[x])
            .ToList();
    }
}