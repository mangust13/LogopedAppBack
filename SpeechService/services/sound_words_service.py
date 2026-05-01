from dataclasses import dataclass


@dataclass
class SoundWord:
    sound: str
    word: str
    position: str


SOUND_WORDS: list[SoundWord] = [
    # Р
    SoundWord("р", "рак", "початок"),
    SoundWord("р", "роза", "початок"),
    SoundWord("р", "тигр", "кінець"),
    SoundWord("р", "комар", "кінець"),

    # Л
    SoundWord("л", "лампа", "початок"),
    SoundWord("л", "лиса", "початок"),
    SoundWord("л", "стіл", "кінець"),
    SoundWord("л", "пенал", "кінець"),

    # С
    SoundWord("с", "сом", "початок"),
    SoundWord("с", "сова", "початок"),
    SoundWord("с", "оса", "середина"),
    SoundWord("с", "ліс", "кінець"),

    # З
    SoundWord("з", "замок", "початок"),
    SoundWord("з", "зуб", "початок"),
    SoundWord("з", "коза", "середина"),
    SoundWord("з", "мороз", "кінець"),

    # Ц
    SoundWord("ц", "цар", "початок"),
    SoundWord("ц", "цирк", "початок"),
    SoundWord("ц", "заєць", "кінець"),
    SoundWord("ц", "палац", "кінець"),

    # ДЗ
    SoundWord("дз", "дзвін", "початок"),
    SoundWord("дз", "дзьоб", "початок"),

    # Ш
    SoundWord("ш", "шар", "початок"),
    SoundWord("ш", "шапка", "початок"),
    SoundWord("ш", "кішка", "середина"),
    SoundWord("ш", "душ", "кінець"),

    # Ж
    SoundWord("ж", "жаба", "початок"),
    SoundWord("ж", "жук", "початок"),
    SoundWord("ж", "їжак", "середина"),
    SoundWord("ж", "ніж", "кінець"),

    # Ч
    SoundWord("ч", "чашка", "початок"),
    SoundWord("ч", "човен", "початок"),
    SoundWord("ч", "ключ", "кінець"),
    SoundWord("ч", "м'яч", "кінець"),

    # ДЖ - специфічний для укр, але слова співзвучні
    SoundWord("дж", "джміль", "початок"),
    SoundWord("дж", "джунглі", "початок"),
]


def get_all() -> list[SoundWord]:
    return SOUND_WORDS


def get_by_sound(sound: str) -> list[SoundWord]:
    return [w for w in SOUND_WORDS if w.sound == sound]


def get_unique_sounds() -> list[str]:
    seen = set()
    return [w.sound for w in SOUND_WORDS if not (w.sound in seen or seen.add(w.sound))]