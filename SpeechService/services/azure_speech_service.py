import tempfile
import os
import json
import asyncio
import azure.cognitiveservices.speech as speechsdk
from pydub import AudioSegment
from config import AZURE_SPEECH_KEY, AZURE_SPEECH_REGION
from models.schemas import AnalyzeResponse, WordResult, PhonemeResult


def _convert_to_wav(input_path: str) -> str:
    wav_fd, wav_path = tempfile.mkstemp(suffix=".wav")
    os.close(wav_fd)
    audio = AudioSegment.from_file(input_path)
    audio = audio.set_frame_rate(16000).set_channels(1).set_sample_width(2)
    audio.export(wav_path, format="wav")
    return wav_path


def _map_phonemes(raw_phonemes: list, word: str) -> list[PhonemeResult]:
    chars = list(word)
    return [
        PhonemeResult(
            phoneme=chars[i] if i < len(chars) else f"#{i}",
            accuracy_score=p.get("PronunciationAssessment", {}).get("AccuracyScore", 0.0),
        )
        for i, p in enumerate(raw_phonemes)
    ]


def _run_assessment(audio_path: str, expected_text: str, target_sound: str) -> AnalyzeResponse:
    wav_path = None
    try:
        wav_path = _convert_to_wav(audio_path)

        speech_config = speechsdk.SpeechConfig(
            subscription=AZURE_SPEECH_KEY,
            region=AZURE_SPEECH_REGION,
        )
        speech_config.speech_recognition_language = "ru-RU"

        pronunciation_config = speechsdk.PronunciationAssessmentConfig(
            reference_text=expected_text,
            grading_system=speechsdk.PronunciationAssessmentGradingSystem.HundredMark,
            granularity=speechsdk.PronunciationAssessmentGranularity.Phoneme,
            enable_miscue=True,
        )

        audio_config = speechsdk.audio.AudioConfig(filename=wav_path)

        recognizer = speechsdk.SpeechRecognizer(
            speech_config=speech_config,
            audio_config=audio_config,
        )

        pronunciation_config.apply_to(recognizer)
        result = recognizer.recognize_once()

        if result.reason == speechsdk.ResultReason.Canceled:
            cancellation = speechsdk.CancellationDetails(result)
            raise RuntimeError(f"Azure canceled: {cancellation.error_details}")

        if result.reason == speechsdk.ResultReason.NoMatch:
            return AnalyzeResponse(
                recognized_text="",
                expected_text=expected_text,
                accuracy_score=0.0,
                pronunciation_score=0.0,
                completeness_score=0.0,
                fluency_score=0.0,
                is_correct=False,
                target_sound=target_sound,
                words=[],
            )

        raw_json = json.loads(
            result.properties.get(
                speechsdk.PropertyId.SpeechServiceResponse_JsonResult, "{}"
            )
        )

        nbest = raw_json.get("NBest", [])

        words: list[WordResult] = []
        accuracy_score = 0.0
        pronunciation_score = 0.0
        completeness_score = 0.0
        fluency_score = 0.0

        if nbest:
            pa = nbest[0].get("PronunciationAssessment", {})
            accuracy_score = pa.get("AccuracyScore", 0.0)
            pronunciation_score = pa.get("PronScore", 0.0)
            completeness_score = pa.get("CompletenessScore", 0.0)
            fluency_score = pa.get("FluencyScore", 0.0)

            for w in nbest[0].get("Words", []):
                word_text = w.get("Word", "")
                phonemes = _map_phonemes(w.get("Phonemes", []), word_text)
                words.append(
                    WordResult(
                        word=word_text,
                        accuracy_score=w.get("PronunciationAssessment", {}).get("AccuracyScore", 0.0),
                        error_type=w.get("PronunciationAssessment", {}).get("ErrorType", "None"),
                        phonemes=phonemes,
                    )
                )

        return AnalyzeResponse(
            recognized_text=result.text,
            expected_text=expected_text,
            accuracy_score=accuracy_score,
            pronunciation_score=pronunciation_score,
            completeness_score=completeness_score,
            fluency_score=fluency_score,
            is_correct=pronunciation_score >= 60.0,
            target_sound=target_sound,
            words=words,
        )
    finally:
        if wav_path and os.path.exists(wav_path):
            try:
                os.unlink(wav_path)
            except OSError:
                pass


async def analyze(audio_bytes: bytes, filename: str, expected_text: str, target_sound: str) -> AnalyzeResponse:
    suffix = os.path.splitext(filename)[-1] or ".m4a"

    tmp_fd, tmp_path = tempfile.mkstemp(suffix=suffix)
    try:
        with os.fdopen(tmp_fd, "wb") as f:
            f.write(audio_bytes)

        return await asyncio.get_event_loop().run_in_executor(
            None, _run_assessment, tmp_path, expected_text, target_sound
        )
    finally:
        try:
            os.unlink(tmp_path)
        except OSError:
            pass