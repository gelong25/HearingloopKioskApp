//using System.Text;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;

using Google.Cloud.Speech.V1;
using NAudio.Wave;
using Grpc.Auth;

namespace HearingloopKioskApp.Windows.Translate
{
    public partial class TranslateZone : Window
    {
        public TranslateZone()
        {
            InitializeComponent();
            InitializeGoogleSpeechClient();
            InitializeMicrophone();
        }

        // TranslateButtonClick 이벤트 핸들러
        private void TranslateButtonClick(object? sender, RoutedEventArgs e)
        {
            MainWindow1 mainWindow = new();
            mainWindow.Show();

            this.Close();
        }

        public SpeechClient? speechClient;

        private WaveInEvent? waveIn1;
        private WaveInEvent? waveIn2;

        // Google Speech-To-Text 클라이언트 초기화 메서드
        private void InitializeGoogleSpeechClient()
        {
            try
            {
                // Google Cloud Speech API 키 경로 환경 변수에서 가져오기
                var apiKeyPath = Environment.GetEnvironmentVariable("Google_Cloud_Speech");

                if (string.IsNullOrEmpty(apiKeyPath))
                {
                    throw new Exception("환경변수가 설정되지 않았습니다.");
                }

                // 자격 증명 생성 및 클라이언트 초기화
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(apiKeyPath)
                    .CreateScoped(SpeechClient.DefaultScopes);
                speechClient = new SpeechClientBuilder { ChannelCredentials = credential.ToChannelCredentials() }.Build();
                Debug.WriteLine("Google Speech-To-Text 클라이언트 초기화 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Google Speech-To-Text 클라이언트 초기화 실패: " + ex.Message);
            }
        }

        // 마이크 초기화 메서드
        private void InitializeMicrophone()
        {
            var waveInDevices = GetAvailableMicrophoneDevices();
            try
            {
                if (waveInDevices.Count > 0)
                {
                    waveIn1 = new WaveInEvent
                    {
                        DeviceNumber = waveInDevices[0],
                        WaveFormat = new WaveFormat(16000, 1) // 16kHz, Mono
                    };
                    waveIn1.DataAvailable += WaveIn1_DataAvailable;
                    waveIn1.StartRecording();
                    Debug.WriteLine("첫 번째 마이크 초기화 성공, 녹음 시작");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"첫 번째 마이크 초기화 실패 {ex.Message}");
            }
            try {
                if (waveInDevices.Count > 1)
                {
                    waveIn2 = new WaveInEvent
                    {
                        DeviceNumber = waveInDevices[1],
                        WaveFormat = new WaveFormat(16000, 1) // 16kHz, Mono
                    };
                    waveIn2.DataAvailable += WaveIn2_DataAvailable;
                    waveIn2.StartRecording();
                    Debug.WriteLine("두 번째 마이크 초기화 성공, 녹음 시작");
                }
         }
            catch(Exception ex)
            {
                Debug.WriteLine($"두 번째 마이크 초기화 실패 {ex.Message}");
            }
        }

        // 사용 가능한 마이크 장치 목록을 가져오는 메서드
        private static List<int> GetAvailableMicrophoneDevices()
        {
            var devices = new List<int>();
            for (int i = 0; i < WaveInEvent.DeviceCount; i++)
            {
                var deviceInfo = WaveInEvent.GetCapabilities(i);
                devices.Add(i);
                Debug.WriteLine($"Device {i}: {deviceInfo.ProductName}");
            }
            return devices;
        }


        // 첫 번째 마이크 데이터 처리 메서드 
        private async void WaveIn1_DataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                var audioBytes = e.Buffer.Take(e.BytesRecorded).ToArray();
                Debug.WriteLine($"첫 번째 마이크 데이터 길이: {audioBytes.Length} 바이트");
                
                var response = await RecognizeSpeech(audioBytes);
                Debug.WriteLine($"첫 번째 마이크 데이터 처리 결과{response}");
                
                Dispatcher.Invoke(() => TransTextBox1.Text += response);
                Debug.WriteLine("첫 번째 마이크 데이터 처리 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"첫 번째 마이크 데이터 처리 실패{ex.Message}");
            }
        }

        // 두 번째 마이크 데이터 처리 메서드 
        private async void WaveIn2_DataAvailable(object? sender, WaveInEventArgs e)
        {
            try
            {
                var audioBytes = e.Buffer.Take(e.BytesRecorded).ToArray();
                Debug.WriteLine($"두 번째 마이크 데이터 길이: {audioBytes: Length} 바이트");

                var response = await RecognizeSpeech(audioBytes);
                Debug.WriteLine($"두 번째 마이크 데이터 처리 결과{response}");

                Dispatcher.Invoke(() => TransTextBox2.Text += response);
                Debug.WriteLine("두 번째 마이크 데이터 처리 완료");
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"두 번째 마이크 데이터 처리 실패{ex.Message}");
            }
        }

        // 음성 인식 메서드
        private async Task<string> RecognizeSpeech(byte[] audioBytes)
        {
            if (speechClient == null)
            {
                throw new InvalidOperationException("SpeechClient가 초기화되지 않았습니다.");
            }

            try {
                // RecognizeSpeech 메서드 호출하여 음성 인식 결과 가져옴 
                var response = await speechClient.RecognizeAsync(new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 16000,
                    LanguageCode = "ko-KR"
                }, RecognitionAudio.FromBytes(audioBytes));

                if (response == null)
                {
                    Debug.WriteLine("음성 인식 응답이 null 입니다.");
                }

                if (!response.Results.Any())
                {
                    Debug.WriteLine("음성 인식 결과가 없습니다.");
                    return string.Empty;
                }

                // 결과를 처리해서 첫 번째 대안의 전사 텍스트 반환
                // 만약 null이면, 빈 문자열 반환 
                var transcript = response.Results
                .SelectMany(result => result.Alternatives)
                .Select(alternative => alternative.Transcript)
                .FirstOrDefault() ?? string.Empty;

                Debug.WriteLine($"음성 인식 성공: {transcript}");
                return transcript;
            }
            catch (Exception ex) 
            {
                Debug.WriteLine($"Speech recognition failed: {ex.Message}");
                return string.Empty;
            }
        }

    }
}
