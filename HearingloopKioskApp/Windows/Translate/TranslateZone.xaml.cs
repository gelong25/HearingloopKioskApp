using System;
using System.Diagnostics;
//using System.IO;
//using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Google.Cloud.Speech.V1;
using NAudio.Wave;
using Grpc.Auth;
//using Grpc.Core;

namespace HearingloopKioskApp.Windows.Translate
{
    public partial class TranslateZone : Window
    {
        private WaveInEvent? waveIn;
        private WaveFileWriter? waveFileWriter;
        private SpeechClient? speechClient;
        private DispatcherTimer? timer;
        private bool isRecording;

        public TranslateZone()
        {
            InitializeComponent();
            InitializeGoogleSpeechClient();
            InitializeMicrophone();
            StartRecording();
        }

        private void TranslateButtonClick(object? sender, RoutedEventArgs e)
        {
            MainWindow1 mainWindow = new();
            mainWindow.Show();
            this.Close();
        }

        private void InitializeGoogleSpeechClient()
        {
            Debug.WriteLine("InitializeGoogleSpeechClient 시작");

            try
            {
                var apiKeyPath = Environment.GetEnvironmentVariable("Google_Cloud_Speech");
                if (string.IsNullOrEmpty(apiKeyPath))
                {
                    throw new Exception("환경변수가 설정되지 않았습니다.");
                }

                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(apiKeyPath)
                    .CreateScoped(SpeechClient.DefaultScopes);
                speechClient = new SpeechClientBuilder { ChannelCredentials = credential.ToChannelCredentials() }.Build();
                Debug.WriteLine("Google Speech-To-Text 클라이언트 초기화 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Google Speech-To-Text 클라이언트 초기화 실패: " + ex.Message);
            }
            Debug.WriteLine("InitializeGoogleSpeechClient 끝");
        }

        private void InitializeMicrophone()
        {
            Debug.WriteLine("InitializeMicrophone 시작");

            try
            {
                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1)
                };
                waveIn.DataAvailable += OnDataAvailable;
                waveIn.RecordingStopped += OnRecordingStopped;

                Debug.WriteLine("마이크 초기화 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("마이크 초기화 실패: " + ex.Message);
            }
            Debug.WriteLine("InitializeMicrophone 끝");
        }

        private void StartRecording()
        {
            Debug.WriteLine("StartRecording 시작");

            if (waveIn != null)
            {
                waveFileWriter = new WaveFileWriter("temp.wav", waveIn.WaveFormat);
                waveIn.StartRecording();
                isRecording = true;

                // 일정 시간 후 녹음 중지
                timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer.Tick += (sender, e) =>
                {
                    if (isRecording)
                    {
                        waveIn.StopRecording();
                    }
                    timer.Stop();
                };
                timer.Start();
            }

            Debug.WriteLine("StartRecording 끝");
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            Debug.WriteLine("OnDataAvailable 시작");

            if (waveFileWriter != null)
            {
                waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveFileWriter.Flush();
            }

            Debug.WriteLine("OnDataAvailable 끝");
        }

        private async void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OnRecordingStopped 시작");

            waveFileWriter?.Dispose();
            waveFileWriter = null;
            isRecording = false;

            await RecognizeSpeechAsync("temp.wav");

            Debug.WriteLine("OnRecordingStopped 끝");

            // 일정 시간 후 다시 녹음 시작
            StartRecording();
        }

        private async Task RecognizeSpeechAsync(string filePath)
        {
            Debug.WriteLine("RecognizeSpeechAsync 시작");
            try
            {
                if (speechClient != null)
                {
                    var response = await speechClient.RecognizeAsync(new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "ko-KR",
                    }, RecognitionAudio.FromFile(filePath));

                    foreach (var result in response.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                TransTextBox1.Text += alternative.Transcript + " ";
                            });
                        }
                    }
                    Debug.WriteLine("음성 인식 성공");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("음성 인식 실패: " + ex.Message);
            }
            Debug.WriteLine("RecognizeSpeechAsync 끝");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            waveIn?.StopRecording();
            timer?.Stop();
        }
    }
}
