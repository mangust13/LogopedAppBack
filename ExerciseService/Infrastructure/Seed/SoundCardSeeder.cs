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

    private static readonly List<(string Sound, string Word, string ImageFile, bool IsAlive)> Cards = new()
    {
        // Звук Р — початок
        ("р", "Ромашка",   "1_chamomile.png",  false),
        ("р", "Рак",       "1_crayfish.png",   true),
        ("р", "Роса",      "1_dew.png",        false),
        ("р", "Риба",      "1_fish.png",       true),
        ("р", "Рука",      "1_hand.png",       true),
        ("р", "Рукавиці",  "1_mittens.png",    false),
        ("р", "Рот",       "1_mouth.png",      true),
        ("р", "Ракетка",   "1_racket.png",     false),
        ("р", "Редиска",   "1_radish.png",     false),
        ("р", "Робот",     "1_robot.png",      false),

        // Звук Р — середина
        ("р", "Береза",    "2_birch.png",      false),
        ("р", "Вареники",  "2_dumplings.png",  false),
        ("р", "Гітара",    "2_guitar.png",     false),
        ("р", "Нора",      "2_hole.png",       false),
        ("р", "Морозиво",  "2_ice_cream.png",  false),
        ("р", "Варення",   "2_jam.png",        false),
        ("р", "Озеро",     "2_lake.png",       false),
        ("р", "Гора",      "2_mountain.png",   false),
        ("р", "Піраміда",  "2_pyramid.png",    false),
        ("р", "Дорога",    "2_road.png",       false),

        // Звук Р — кінець
        ("р", "Актор",     "3_actor.png",      true),
        ("р", "Пекар",     "3_baker.png",      true),
        ("р", "Бобер",     "3_beaver.png",     true),
        ("р", "Касир",     "3_cashier.png",    true),
        ("р", "Хор",       "3_choir.png",      true),
        ("р", "Мухомор",   "3_mushroom.png",   false),
        ("р", "Димар",     "3_funnel.png",     false),
        ("р", "Комар",     "3_mosquito.png",   true),
        ("р", "Сир",       "3_cheese.png",     false),
        ("р", "Цукор",     "3_sugar.png",      false),

        // Звук Р — збіг
        ("р", "Ґрати",     "4_bars.png",       false),
        ("р", "Хрущ",      "4_chafer.png",     true),
        ("р", "Краб",      "4_crab.png",       true),
        ("р", "Брови",     "4_eyebrows.png",   false),
        ("р", "Фрукти",    "4_fruits.png",     false),
        ("р", "Кропива",   "4_nettle.png",     false),
        ("р", "Цифри",     "4_numbers.png",    false),
        ("р", "Грабіжник", "4_robber.png",     true),
        ("р", "Черв'як",   "4_worm.png",       true),
        ("р", "Зебра",     "4_zebra.png",      true),

        // Звук Л — початок
        ("л", "Лук",       "1_bow.png",              false),
        ("л", "Лама",      "1_lama.png",             true),
        ("л", "Лампа",     "1_lamp.png",             false),
        ("л", "Листок",    "1_leaf.png",             false),
        ("л", "Лист",      "1_letter.png",           false),
        ("л", "Лупа",      "1_dandruff.png",         false),
        ("л", "Лак",       "1_varnish.png",          false),
        ("л", "Лапа",      "1_paw.png",              false),
        ("л", "Лопата",    "1_shovel.png",           false),
        ("л", "Ложка",     "1_spoon.png",            false),

        // Звук Л — середина
        ("л", "Жолудь",    "2_acorn.png",       false),
        ("л", "Голуб",     "2_dove.png",        true),
        ("л", "Молоток",   "2_hammer.png",      false),
        ("л", "Соловей",   "2_nightingale.png", true),
        ("л", "Халат",     "2_robe.png",        false),
        ("л", "Салат",     "2_salad.png",       false),
        ("л", "Акула",     "2_shark.png",       true),
        ("л", "Мило",      "2_soap.png",        false),
        ("л", "Село",      "2_village.png",     false),
        ("л", "Колесо",    "2_wheel.png",       false),

        // Звук Л — кінець
        ("л", "Бал",       "3_ball.png",         false),
        ("л", "Чохол",     "3_case.png",         false),
        ("л", "Пил",       "3_dust.png",         false),
        ("л", "Футбол",    "3_football.png",     false),
        ("л", "Бокал",     "3_glass.png",        false),
        ("л", "Вузол",     "3_knot.png",         false),
        ("л", "Овал",      "3_oval.png",         false),
        ("л", "Пенал",     "3_pencil_case.png",  false),
        ("л", "Факел",     "3_torch.png",        false),
        ("л", "Вокзал",    "3_train_station.png",false),

        // Звук Л — збіг
        ("л", "Яблуко",    "4_apple.png",    false),
        ("л", "Блузка",    "4_blouse.png",   false),
        ("л", "Масло",     "4_butter.png",   false),
        ("л", "Злива",     "4_downpour.png", false),
        ("л", "Слон",      "4_elephant.png", true),
        ("л", "Фламінго",  "4_flamingo.png", true),
        ("л", "Клей",      "4_glue.png",     false),
        ("л", "Клен",      "4_maple.png",    false),
        ("л", "Слива",     "4_plum.png",     false),
        ("л", "Плащ",      "4_raincoat.png", false),
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
                IsAlive = c.IsAlive,
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