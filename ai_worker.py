import json
import pika
import os
import sys
from allosaurus.app import read_recognizer

from epitran import Epitran
import Levenshtein

# 1. Завантажуємо моделі (це станеться один раз при запуску)
print("⏳ Завантаження AI моделей...")
# Allosaurus слухає звук і видає фонеми. 'ukr' - код для української
allo_model = read_recognizer() 
# Epitran переводить текст в ідеальні фонеми
epi_model = Epitran('ukr-Cyrl') 

print("✅ Моделі завантажені. Підключення до RabbitMQ...")

# Налаштування RabbitMQ
RABBIT_HOST = os.getenv('RABBITMQ_HOST', 'localhost')
EXCHANGE_NAME = 'speech_exchange'
QUEUE_NAME = 'exercise.audio'

connection = pika.BlockingConnection(pika.ConnectionParameters(host=RABBIT_HOST))
channel = connection.channel()

channel.exchange_declare(exchange=EXCHANGE_NAME, exchange_type='topic')
channel.queue_declare(queue=QUEUE_NAME, durable=False)
channel.queue_bind(exchange=EXCHANGE_NAME, queue=QUEUE_NAME, routing_key='exercise.audio.*')

def analyze_audio(audio_path, reference_text):
    """
    Магія тут.
    """
    # 1. Отримуємо "Ідеальні" фонеми з тексту (що мало бути)
    # epi_model видає IPA string. Наприклад "риба" -> "r ɪ b a"
    target_ipa = epi_model.transliterate(reference_text)
    
    # 2. Отримуємо "Реальні" фонеми з аудіо (що сказав юзер)
    # Allosaurus повертає рядок IPA. Наприклад "l ɪ b a"
    try:
        # lang_id='ukr' підказує моделі, які звуки пріоритетні, але НЕ включає словник
        actual_ipa = allo_model.recognize(audio_path, lang_id='ukr')
    except Exception as e:
        print(f"Error recognising audio: {e}")
        return None

    # 3. Порівнюємо два рядки фонем
    # Levenshtein distance покаже різницю в символах
    distance = Levenshtein.distance(target_ipa, actual_ipa)
    max_len = max(len(target_ipa), len(actual_ipa))
    
    accuracy = 0
    if max_len > 0:
        accuracy = (1 - distance / max_len) * 100

    # 4. Формуємо детальний звіт (для підсвітки помилок)
    # Тут можна використати editops, щоб знайти конкретно де 'r' замінилась на 'l'
    edits = Levenshtein.editops(target_ipa, actual_ipa)
    
    errors = []
    for op, src_i, dest_i in edits:
        if op == 'replace':
            errors.append({
                "Expected": target_ipa[src_i],
                "Actual": actual_ipa[dest_i],
                "Type": "Mispronunciation"
            })
        elif op == 'delete':
             errors.append({
                "Expected": target_ipa[src_i],
                "Actual": "-",
                "Type": "Omission"
            })

    return {
        "RecognizedIPA": actual_ipa,
        "ReferenceIPA": target_ipa,
        "Accuracy": accuracy,
        "Errors": errors
    }

def callback(ch, method, properties, body):
    print(f" [x] Отримано повідомлення")
    data = json.loads(body)
    
    exercise_id = data.get("ExerciseId")
    audio_path = data.get("AudioUrl")
    text = data.get("ReferenceText")

    print(f" --- Обробка: '{text}' ---")
    
    result = analyze_audio(audio_path, text)
    
    if result:
        response = {
            "ExerciseId": exercise_id,
            "RecognizedIPA": result['RecognizedIPA'], # Можеш показати юзеру транскрипцію!
            "AccuracyScore": result['Accuracy'],
            "PronunciationErrors": result['Errors'],
            "Feedback": f"Точність: {result['Accuracy']:.1f}%. Почуто: [{result['RecognizedIPA']}]"
        }
        
        # Відправка назад в RabbitMQ (в іншу чергу або з іншим ключем)
        ch.basic_publish(
            exchange=EXCHANGE_NAME,
            routing_key='speech.result.done',
            body=json.dumps(response)
        )
        print(" [x] Результат відправлено")
    else:
        print(" [!] Помилка обробки")

channel.basic_consume(queue=QUEUE_NAME, on_message_callback=callback, auto_ack=True)

print(' [*] Python AI Worker працює. Чекаю чергу...')
channel.start_consuming()