from dataclasses import dataclass


@dataclass
class SoundWord:
    sound: str
    word: str
    position: str


SOUND_WORDS: list[SoundWord] = [
    # Р
    SoundWord("р", "рак", "початок"),
    SoundWord("р", "тигр", "кінець"),

    # Л
    SoundWord("л", "лампа", "початок"),
    SoundWord("л", "пенал", "кінець"),

    # С
    SoundWord("с", "сом", "початок"),
    SoundWord("с", "оса", "середина"),

    # З
    SoundWord("з", "зуб", "початок"),
    SoundWord("з", "мороз", "кінець"),

    # Ц
    SoundWord("ц", "цар", "початок"),
    SoundWord("ц", "цирк", "початок"),

    # ДЗ
    SoundWord("дз", "дзвін", "початок"),
    SoundWord("дз", "дзьоб", "початок"),

    # Ш
    SoundWord("ш", "шапка", "початок"),
    SoundWord("ш", "душ", "кінець"),

    # Ж
    SoundWord("ж", "жаба", "початок"),
    SoundWord("ж", "жук", "початок"),

    # Ч
    SoundWord("ч", "чашка", "початок"),
    SoundWord("ч", "човен", "початок"),

    # ДЖ - специфічний для укр
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