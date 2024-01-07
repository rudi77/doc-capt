import pika
import json
import base64
import pytesseract
from PIL import Image, UnidentifiedImageError
from pdf2image import convert_from_bytes
from io import BytesIO
import uuid

def process_image(image):
    """
    Process image and return OCR result.
    @param image: Image data as bytes.
    """

    # Perform OCR using pytesseract
    ocr_result = pytesseract.image_to_string(image)

    return ocr_result

def convert_pdf_to_images(pdf_data):
    """
    Convert PDF data to a list of images.
    """

    # Convert PDF to images using pdf2image. Use 300 DPI and png format
    images = convert_from_bytes(pdf_data, dpi=300, fmt='png')
    
    return images

def process_document(document):
    """
    Process document data and return OCR result.
    @param document_data: Document data as base64 encoded string.
    """
    
    # Decode base64 encoded document data
    decoded_data = base64.b64decode(document['documentData'])

    # Check if the document is a PDF
    if decoded_data[0:4] == b'%PDF':
        # Convert PDF to images
        images = convert_pdf_to_images(decoded_data)

        # Process each image
        ocr_result = ''
        for image in images:
            ocr_result += process_image(image)
    else:
        # Process image
        ocr_result = process_image(decoded_data)

    return ocr_result



def callback(ch, method, properties, body):
    """
    Callback function to process received messages. 
    The body is a masstransit envelope. The message property contains the actual base64 encoded document data.
    @param ch: Channel object
    @param method: Method object
    @param properties: Properties object
    @param body: Message body which is the mass transit envelope
    """
        
    # Decode body
    envelope = json.loads(body)

    # Get document data
    document_data = envelope['message']

    # Process document
    ocr_result = process_document(document_data)

    # publish an OcrCompleted event which has the following format
    # public class OcrCompleted
    # {
    #     public OcrCompleted(string instanceId, Guid correlationId, string text);
    #     public string InstanceId { get; set; }
    #     public Guid CorrelationId { get; set; }
    #     public string Text { get; set; }
    # }

    # Create OcrCompleted event
    ocr_completed = {
        'instanceId': envelope['message']['instanceId'],
        'correlationId': envelope['message']['correlationId'],
        'text': ocr_result
    }

    # Publish OcrCompleted event to RabbitMQ server
    channel.basic_publish(exchange='', routing_key='elsa-ocr-completed', body=json.dumps(ocr_completed))
    


# Establish a connection to RabbitMQ server
connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
channel = connection.channel()

# Declare a queue to receive OCR results
queue_name = 'ocr-queue'
channel.queue_declare(queue=queue_name, durable=True)

# Subscribe to the queue
channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)

print('Waiting for messages. To exit press CTRL+C')
try:
    channel.start_consuming()
except KeyboardInterrupt:
    channel.stop_consuming()
connection.close()
