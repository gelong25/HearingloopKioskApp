using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using NAudio.Wave;

namespace HearingloopKioskApp.Windows.Translate
{
    public partial class TranslateZone : Window
    {
        // 마이크 입력 처리 
        private WaveInEvent? waveIn1;
        private WaveInEvent? waveIn2;
        
        // 오디오 파일 
        private WaveFileWriter? waveFileWriter1;
        private WaveFileWriter? waveFileWriter2;
        
        // Google Speech-To-Text 클라이언트
        private SpeechClient? speechClient;
        
        // 녹음 타이머 
        private DispatcherTimer? timer1;
        private DispatcherTimer? timer2;
        
        // 마이크 녹음 상태 
        private bool isRecording1;
        private bool isRecording2;

        public TranslateZone()
        {
            InitializeComponent();
            InitializeGoogleSpeechClient();
            InitializeMicrophone();
            StartRecording1();
            StartRecording2();
        }

        private void TranslateButtonClick(object? sender, RoutedEventArgs e)
        {

            StopRecording();

            MainWindow1 mainWindow = new();
            mainWindow.Show();
            this.Close();
        }

        // Google Speech-To-Text 클라이언트 초기화 
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

        // 마이크 초기화 
        private void InitializeMicrophone()
        {
            Debug.WriteLine("InitializeMicrophone 시작");

            try
            {
                waveIn1 = new WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new WaveFormat(16000, 1)
                };
                waveIn1.DataAvailable += OnDataAvailable1;
                waveIn1.RecordingStopped += OnRecordingStopped1;

                if (WaveIn.DeviceCount > 1)
                {
                    waveIn2 = new WaveInEvent
                    {
                        DeviceNumber = 1,
                        WaveFormat = new WaveFormat(16000, 1)
                    };
                    waveIn2.DataAvailable += OnDataAvailable2;
                    waveIn2.RecordingStopped += OnRecordingStopped2;
                }

                Debug.WriteLine("마이크 초기화 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("마이크 초기화 실패: " + ex.Message);
            }
            Debug.WriteLine("InitializeMicrophone 끝");
        }

        // 첫 번째 마이크 녹음 시작 
        private void StartRecording1()
        {
            Debug.WriteLine("StartRecording1 시작");

            if (waveIn1 != null)
            {
                waveFileWriter1 = new WaveFileWriter("temp1.wav", waveIn1.WaveFormat);
                waveIn1.StartRecording();
                isRecording1 = true;

                timer1 = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer1.Tick += (sender, e) =>
                {
                    if (isRecording1)
                    {
                        waveIn1.StopRecording();
                    }
                    timer1.Stop();
                };
                timer1.Start();
            }

            Debug.WriteLine("StartRecording1 끝");
        }

        // 두 번째 마이크 녹음 시작 
        private void StartRecording2()
        {
            Debug.WriteLine("StartRecording2 시작");

            if (waveIn2 != null)
            {
                waveFileWriter2 = new WaveFileWriter("temp2.wav", waveIn2.WaveFormat);
                waveIn2.StartRecording();
                isRecording2 = true;

                timer2 = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timer2.Tick += (sender, e) =>
                {
                    if (isRecording2)
                    {
                        waveIn2.StopRecording();
                    }
                    timer2.Stop();
                };
                timer2.Start();
            }

            Debug.WriteLine("StartRecording2 끝");
        }

        private void OnDataAvailable1(object? sender, WaveInEventArgs e)
        {
            Debug.WriteLine("OnDataAvailable1 시작");

            if (waveFileWriter1 != null)
            {
                waveFileWriter1.Write(e.Buffer, 0, e.BytesRecorded);
                waveFileWriter1.Flush();
            }

            Debug.WriteLine("OnDataAvailable1 끝");
        }

        private void OnDataAvailable2(object? sender, WaveInEventArgs e)
        {
            Debug.WriteLine("OnDataAvailable2 시작");

            if (waveFileWriter2 != null)
            {
                waveFileWriter2.Write(e.Buffer, 0, e.BytesRecorded);
                waveFileWriter2.Flush();
            }

            Debug.WriteLine("OnDataAvailable2 끝");
        }

        // 첫 번째 마이크 녹음 중지 
        private async void OnRecordingStopped1(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OnRecordingStopped1 시작");

            waveFileWriter1?.Dispose();
            waveFileWriter1 = null;
            isRecording1 = false;

            await RecognizeSpeechAsync("temp1.wav", 1);

            Debug.WriteLine("OnRecordingStopped1 끝");

            StartRecording1();
        }

        // 두 번째 마이크 녹음 중지 
        private async void OnRecordingStopped2(object? sender, StoppedEventArgs e)
        {
            Debug.WriteLine("OnRecordingStopped2 시작");

            waveFileWriter2?.Dispose();
            waveFileWriter2 = null;
            isRecording2 = false;

            await RecognizeSpeechAsync("temp2.wav", 2);

            Debug.WriteLine("OnRecordingStopped2 끝");

            StartRecording2();
        }

        private async Task RecognizeSpeechAsync(string filePath, int microphoneIndex)
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
                                if (microphoneIndex == 1)
                                {
                                    TransTextBox1.Text += alternative.Transcript + " "; // 음성 인식 결과를 텍스트 박스1에 추가 
                                }
                                else
                                {
                                    TransTextBox2.Text += alternative.Transcript + " "; // 음성 인식 결과를 텍스트 박스2에 추가 
                                }
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

        private void StopRecording()
        {
            if (waveIn1 != null)
            {
                waveIn1.StopRecording();
                waveIn1.DataAvailable -= OnDataAvailable1;
                waveIn1.RecordingStopped -= OnRecordingStopped1;
                waveIn1.Dispose();
            }

            if (waveIn2 != null)
            {
                waveIn2.StopRecording();
                waveIn2.DataAvailable -= OnDataAvailable2;
                waveIn2.RecordingStopped -= OnRecordingStopped2;
                waveIn2.Dispose();
            }
            
            timer1?.Stop();
            timer2?.Stop();
        }

        // 창이 닫힐 때 호출되는 이벤트 핸들러 
        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    waveIn1?.StopRecording();
        //    waveIn2?.StopRecording();
            
        //    timer1?.Stop();
        //    timer2?.Stop();

        //    waveIn1?.Dispose();
        //    waveIn2?.Dispose();
        //}
    }
}
