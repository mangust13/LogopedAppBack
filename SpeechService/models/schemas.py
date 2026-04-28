from pydantic import BaseModel


class PhonemeResult(BaseModel):
    phoneme: str
    accuracy_score: float


class WordResult(BaseModel):
    word: str
    accuracy_score: float
    error_type: str
    phonemes: list[PhonemeResult]


class AnalyzeResponse(BaseModel):
    recognized_text: str
    expected_text: str
    accuracy_score: float
    pronunciation_score: float
    completeness_score: float
    fluency_score: float
    is_correct: bool
    target_sound: str
    words: list[WordResult]


class SoundWordResponse(BaseModel):
    sound: str
    word: str
    position: str


class SoundWordsGrouped(BaseModel):
    sound: str
    words: list[SoundWordResponse]