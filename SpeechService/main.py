from fastapi import FastAPI, UploadFile, File, Form, HTTPException
from models.schemas import AnalyzeResponse, SoundWordResponse, SoundWordsGrouped
from services import azure_speech_service, sound_words_service
import config

app = FastAPI(title="SpeechService")


@app.get("/health")
async def health():
    return {"status": "ok", "service": "SpeechService"}


@app.get("/sound-words", response_model=list[SoundWordResponse])
async def get_sound_words():
    return [
        SoundWordResponse(sound=w.sound, word=w.word, position=w.position)
        for w in sound_words_service.get_all()
    ]


@app.get("/sound-words/grouped", response_model=list[SoundWordsGrouped])
async def get_sound_words_grouped():
    sounds = sound_words_service.get_unique_sounds()
    return [
        SoundWordsGrouped(
            sound=s,
            words=[
                SoundWordResponse(sound=w.sound, word=w.word, position=w.position)
                for w in sound_words_service.get_by_sound(s)
            ],
        )
        for s in sounds
    ]


@app.post("/analyze", response_model=AnalyzeResponse)
async def analyze_speech(
    audio: UploadFile = File(...),
    expected_word: str = Form(...),
    target_sound: str = Form(...),
):
    if not audio.filename:
        raise HTTPException(status_code=400, detail="Файл не надано")

    audio_bytes = await audio.read()

    if len(audio_bytes) == 0:
        raise HTTPException(status_code=400, detail="Файл порожній")

    return await azure_speech_service.analyze(
        audio_bytes,
        audio.filename,
        expected_word,
        target_sound,
    )