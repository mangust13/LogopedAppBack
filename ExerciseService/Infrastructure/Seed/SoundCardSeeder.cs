using ExerciseService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Infrastructure.Seed;

public static class SoundCardSeeder
{
    private static readonly List<SoundPosition> Positions = new()
    {
        new() { Code = 1, DisplayName = "Початок слова" },
        new() { Code = 2, DisplayName = "Середина слова" },
        new() { Code = 3, DisplayName = "Кінець слова" },
        new() { Code = 4, DisplayName = "Збіг приголосних" },
    };

    private static readonly List<(string Sound, string Word, string ImageFile)> Cards = new()
    {
        ("р", "Ромашка",   "1_chamomile.png"),
        ("р", "Рак",       "1_crayfish.png"),
        ("р", "Роса",      "1_dew.png"),
        ("р", "Риба",      "1_fish.png"),
        ("р", "Рука",      "1_hand.png"),
        ("р", "Рукавиці",  "1_mittens.png"),
        ("р", "Рот",       "1_mouth.png"),
        ("р", "Ракетка",   "1_racket.png"),
        ("р", "Редиска",   "1_radish.png"),
        ("р", "Робот",     "1_robot.png"),

        ("р", "Береза",    "2_birch.png"),
        ("р", "Вареники",  "2_dumplings.png"),
        ("р", "Гітара",    "2_guitar.png"),
        ("р", "Нора",      "2_hole.png"),
        ("р", "Морозиво",  "2_ice_cream.png"),
        ("р", "Варення",   "2_jam.png"),
        ("р", "Озеро",     "2_lake.png"),
        ("р", "Гора",      "2_mountain.png"),
        ("р", "Піраміда",  "2_pyramid.png"),
        ("р", "Дорога",    "2_road.png"),

        ("р", "Актор",     "3_actor.png"),
        ("р", "Пекар",     "3_baker.png"),
        ("р", "Бобер",     "3_beaver.png"),
        ("р", "Касир",     "3_cashier.png"),
        ("р", "Хор",       "3_choir.png"),
        ("р", "Мухомор",   "3_mushroom.png"),
        ("р", "Лійка",     "3_funnel.png"),
        ("р", "Комар",     "3_mosquito.png"),
        ("р", "Сир",       "3_cheese.png"),
        ("р", "Цукор",     "3_sugar.png"),

        ("р", "Ґрати",     "4_bars.png"),
        ("р", "Хрущ",      "4_chafer.png"),
        ("р", "Краб",      "4_crab.png"),
        ("р", "Брови",     "4_eyebrows.png"),
        ("р", "Фрукти",    "4_fruits.png"),
        ("р", "Кропива",   "4_nettle.png"),
        ("р", "Цифри",     "4_numbers.png"),
        ("р", "Грабіжник", "4_robber.png"),
        ("р", "Черв'як",   "4_worm.png"),
        ("р", "Зебра",     "4_zebra.png"),
    };

    public static async Task SeedAsync(ExerciseDbContext db)
    {
        if (!await db.SoundPositions.AnyAsync())
        {
            await db.SoundPositions.AddRangeAsync(Positions);
            await db.SaveChangesAsync();
        }

        if (await db.SoundCards.AnyAsync()) return;

        var positionMap = await db.SoundPositions
            .ToDictionaryAsync(p => p.Code, p => p.Id);

        var cards = Cards.Select(c =>
        {
            var positionCode = ParsePositionFromFilename(c.ImageFile);
            return new SoundCard
            {
                Sound = c.Sound,
                Word = c.Word,
                ImageFile = c.ImageFile,
                PositionId = positionMap[positionCode],
            };
        }).ToList();

        await db.SoundCards.AddRangeAsync(cards);
        await db.SaveChangesAsync();
    }

    private static int ParsePositionFromFilename(string filename)
    {
        var withoutExt = Path.GetFileNameWithoutExtension(filename);
        var firstUnderscore = withoutExt.IndexOf('_');

        if (firstUnderscore > 0 && int.TryParse(withoutExt[..firstUnderscore], out var pos))
            return pos;

        return 1;
    }
}