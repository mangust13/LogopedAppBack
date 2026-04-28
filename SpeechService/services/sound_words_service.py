from dataclasses import dataclass


@dataclass
class SoundWord:
    sound: str
    word: str
    position: str


SOUND_WORDS: list[SoundWord] = [
    SoundWord("р", "риба", "початок"),
    SoundWord("р", "тигр", "кінець"),
    SoundWord("л", "лопата", "початок"),
    SoundWord("л", "стіл", "кінець"),
    SoundWord("с", "сонце", "початок"),
    SoundWord("с", "оса", "середина"),
    SoundWord("ш", "шапка", "початок"),
    SoundWord("ш", "кішка", "середина"),
    SoundWord("ж", "жаба", "початок"),
    SoundWord("ж", "їжак", "середина"),
    SoundWord("з", "зима", "початок"),
    SoundWord("з", "коза", "середина"),
    SoundWord("ч", "чашка", "початок"),
    SoundWord("ч", "ключ", "кінець"),
    SoundWord("ц", "цибуля", "початок"),
    SoundWord("ц", "заєць", "кінець"),
    SoundWord("к", "кіт", "початок"),
    SoundWord("к", "рука", "середина"),
    SoundWord("г", "гора", "початок"),
    SoundWord("г", "нога", "кінець"),
    SoundWord("х", "хліб", "початок"),
    SoundWord("х", "муха", "середина"),
    SoundWord("м", "мама", "початок"),
    SoundWord("м", "зима", "кінець"),
    SoundWord("н", "ніс", "початок"),
    SoundWord("н", "вікно", "кінець"),
    SoundWord("п", "папуга", "початок"),
    SoundWord("п", "суп", "кінець"),
    SoundWord("б", "батько", "початок"),
    SoundWord("б", "риба", "середина"),
    SoundWord("т", "тато", "початок"),
    SoundWord("т", "кіт", "кінець"),
    SoundWord("д", "дім", "початок"),
    SoundWord("д", "вода", "середина"),
    SoundWord("в", "вовк", "початок"),
    SoundWord("в", "трава", "кінець"),
    SoundWord("ф", "фарба", "початок"),
    SoundWord("ф", "шарф", "кінець"),
]


def get_all() -> list[SoundWord]:
    return SOUND_WORDS


def get_by_sound(sound: str) -> list[SoundWord]:
    return [w for w in SOUND_WORDS if w.sound == sound]


def get_unique_sounds() -> list[str]:
    seen = set()
    return [w.sound for w in SOUND_WORDS if not (w.sound in seen or seen.add(w.sound))]