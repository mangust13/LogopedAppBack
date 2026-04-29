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
]


def get_all() -> list[SoundWord]:
    return SOUND_WORDS


def get_by_sound(sound: str) -> list[SoundWord]:
    return [w for w in SOUND_WORDS if w.sound == sound]


def get_unique_sounds() -> list[str]:
    seen = set()
    return [w.sound for w in SOUND_WORDS if not (w.sound in seen or seen.add(w.sound))]