using System.IO;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace HuggingFace.API.Examples {
     public class SpeechRecognitionExample : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        public GameObject spider;  // Asigna el objeto de la araña desde el Inspector

        private AudioClip clip;
        private byte[] bytes;
        private bool recording;

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                StartRecording();
            }

            if (recording && Input.GetKeyDown(KeyCode.S)) {
                StopRecording();
            }
        }

        private void StartRecording() {
            text.color = Color.white;
            text.text = "Recording...";
            clip = Microphone.Start(null, false, 10, 44100);
            recording = true;
        }

        private void StopRecording() {
        try {
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
            recording = false;
            SendRecording();
        } catch (Exception e) {
            Debug.LogError($"Error during StopRecording: {e.Message}");
        }
        }


        




        private void SendRecording() {
            text.color = Color.yellow;
            text.text = "Sending...";
            HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            text.color = Color.white;
            text.text = response;

                // Procesa la salida para mover la araña a la izquierda si es necesario
                ProcessSpeechOutput(response);
            }, error => {
                text.color = Color.red;
                text.text = error;
            });
        }

        private void ProcessSpeechOutput(string output) {
                        text.text = output;

            // Agrega tu lógica para interpretar la salida del reconocimiento de voz aquí

            if (output.ToLower().Contains("left")) {
                text.text = "moviendo izquierda...";

                MoveSpiderLeft();
            }
            if (output.ToLower().Contains("right")) {
                text.text = "moviendo derecha...";

                MoveSpiderRight();
            }

        }

        private void MoveSpiderLeft() {
            // Mueve la araña a la izquierda (ajusta según tu implementación)
            spider.transform.Translate(Vector3.left * Time.deltaTime * 10.0f);
        }
        private void MoveSpiderRight() {
    // Mueve la araña a la derecha (ajusta según tu implementación)
        spider.transform.Translate(Vector3.right * Time.deltaTime * 10.0f);
        }

        

        private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
            using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
                using (var writer = new BinaryWriter(memoryStream)) {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples) {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return memoryStream.ToArray();
            }
        }
    }
}