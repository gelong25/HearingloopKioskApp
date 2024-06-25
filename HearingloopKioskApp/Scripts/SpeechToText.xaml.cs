using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using Google.Cloud.Speech.V1;
using NAudio.Wave;
using Grpc.Auth;
//using Google.Api.Gax.Grpc;
//using System.Threading;
//using Grpc.Core; 

namespace HearingloopKioskApp.Scripts
{
    public partial class SpeechToText : Window
    {


        // 마이크 장치 ID 변수
        private string? microphoneID = null;
        private string? microphoneID2 = null;

        // WaveInEvent 객체를 통해 마이크 입력 처리
        private WaveInEvent? waveIn1;
        private WaveInEvent? waveIn2;

        // Google Cloud Speech-to-Text 클라이언트
        private SpeechClient? speechClient;

        // 스트리밍 음성 인식 요청 스트림
        private SpeechClient.StreamingRecognizeStream? streamingCall1;
        private SpeechClient.StreamingRecognizeStream? streamingCall2;

        // 텍스트 출력을 위한 TextBlock
        public TextBlock? textbox1, textbox2;



        public SpeechToText()
        {

            InitializeGoogleSpeechClient(); // Google Speech-To-Text 클라이언트 초기화
            InitializeMicrophones(); // 마이크 초기화
        }

        // Google Speech-To-Text 클라이언트 초기화 메서드
        private void InitializeGoogleSpeechClient()
        {
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
        }

        // 마이크 초기화 메서드
        private void InitializeMicrophones()
        {
            try
            {
                if (WaveIn.DeviceCount > 0)
                {
                    // 첫 번째 마이크 ID 저장
                    microphoneID = WaveIn.GetCapabilities(0).ProductName;
                    if (WaveIn.DeviceCount > 1)
                    {
                        // 두 번째 마이크 ID 저장
                        microphoneID2 = WaveIn.GetCapabilities(1).ProductName;
                    }
                }

                // 사용 가능한 모든 마이크 장치 정보 출력
                foreach (var i in Enumerable.Range(0, WaveIn.DeviceCount))
                {
                    var deviceInfo = WaveIn.GetCapabilities(i);
                    Debug.WriteLine("MicroPhone: " + deviceInfo.ProductName);
                }
                Debug.WriteLine("마이크 초기화 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("마이크 초기화 실패: " + ex.Message);
            }
        }

        // 음성 인식 활성화 메서드
        public void OnEnable()
        {
            if (textbox1 != null) textbox1.Text = "";
            if (textbox2 != null) textbox2.Text = "";

            if (!string.IsNullOrEmpty(microphoneID)) StartRecording(0);
            if (!string.IsNullOrEmpty(microphoneID2)) StartRecording(1);
        }

        // 음성 인식 비활성화 메서드
        public void OnDisable()
        {
            if (textbox1 != null) textbox1.Text = "";
            if (textbox2 != null) textbox2.Text = "";
            StopRecording();
        }



        // 녹음 시작 메서드
        private void StartRecording(int deviceIndex)
        {
            try
            {
                var waveIn = new WaveInEvent
                {
                    DeviceNumber = deviceIndex,
                    WaveFormat = new WaveFormat(16000, 1) // 샘플 레이트와 채널 수 설정
                };

                if (deviceIndex == 0)
                {
                    waveIn1 = waveIn;
                    waveIn1.DataAvailable += OnDataAvailable1; // 첫 번째 마이크 데이터 처리 이벤트 핸들러
                    Debug.WriteLine("첫 번째 마이크 녹음 시작");
                }
                else
                {
                    waveIn2 = waveIn;
                    waveIn2.DataAvailable += OnDataAvailable2; // 두 번째 마이크 데이터 처리 이벤트 핸들러
                    Debug.WriteLine("두 번째 마이크 녹음 시작");
                }

                waveIn.StartRecording(); // 녹음 시작
                Debug.WriteLine("녹음 시작 호출 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("녹음 시작 실패: " + ex.Message);
            }
        }

        // 녹음을 중지하는 메서드
        private void StopRecording()
        {
            try
            {
                waveIn1?.StopRecording(); // 첫 번째 마이크 녹음 중지
                waveIn2?.StopRecording(); // 두 번째 마이크 녹음 중지
                Debug.WriteLine("녹음 중지 성공");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("녹음 중지 실패: " + ex.Message);
            }
        }


        // 첫 번째 마이크 데이터 처리 메서드
        private async void OnDataAvailable1(object? sender, WaveInEventArgs e)
        {
            Debug.WriteLine("첫 번째 마이크 데이터 처리 시작");
            try
            {
                await StreamAudioAsync(e.Buffer, e.BytesRecorded, 1); // 스트리밍 음성 인식 요청
                Debug.WriteLine("첫 번째 마이크 데이터 처리 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("첫 번째 마이크 데이터 처리 실패: " + ex.Message);
            }
        }

            // 두 번째 마이크 데이터 처리 메서드
            private async void OnDataAvailable2(object? sender, WaveInEventArgs e)
        {
            Debug.WriteLine("두 번째 마이크 데이터 처리 시작");
            try
            {
                await StreamAudioAsync(e.Buffer, e.BytesRecorded, 2); // 스트리밍 음성 인식 요청
                Debug.WriteLine("두 번째 마이크 데이터 처리 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("두 번째 마이크 데이터 처리 실패: " + ex.Message);
            }
        }

        // 스트리밍 음성 인식 요청 메서드
        private async Task StreamAudioAsync(byte[] buffer, int bytesRecorded, int microphoneIndex)
        {
            Debug.WriteLine($"마이크 {microphoneIndex} 스트리밍 시작");

            try
            {
                // 마이크 인덱스에 따라 해당하는 스트리밍 호출 객체를 선택
                var streamingCall = microphoneIndex == 1 ? streamingCall1 : streamingCall2;

                // 스트리밍 호출 객체가 null이거나 응답 스트림이 더 이상 이동하지 않는 경우, 새로운 스트리밍 호출 객체 생성
                if (streamingCall == null)
                {
                    // SpeechClient의 StreamingRecognize 메서드를 호출하여 스트리밍 호출 객체 생성
                    streamingCall = speechClient!.StreamingRecognize();

                    // 스트리밍 요청을 초기화하기 위한 요청 전송
                    await streamingCall.WriteAsync(new StreamingRecognizeRequest
                    {
                        StreamingConfig = new StreamingRecognitionConfig
                        {
                            Config = new RecognitionConfig
                            {
                                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                                SampleRateHertz = 16000,
                                LanguageCode = "ko-KR"
                            },
                            InterimResults = true
                        }
                    });

                    // 마이크 인덱스에 따라 해당 스트리밍 호출 객체를 저장
                    if (microphoneIndex == 1)
                    {
                        streamingCall1 = streamingCall; // 첫 번째 마이크 스트리밍 호출 객체 저장
                    }
                    else
                    {
                        streamingCall2 = streamingCall; // 두 번째 마이크 스트리밍 호출 객체 저장
                    }

                    Debug.WriteLine($"마이크 {microphoneIndex} 스트리밍 초기화 완료");
                }

                // 오디오 데이터를 스트리밍 요청에 쓰기
                await streamingCall.WriteAsync(new StreamingRecognizeRequest
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRecorded)
                });

                // 음성 인식 결과 처리
                var responseStream = streamingCall.GetResponseStream();

                while (await responseStream.MoveNextAsync(default))           // 스트리밍 응답을 계속해서 처리 
                {
                    var response = responseStream.Current;                    // 현재 응답 가져오기 

                    foreach (var result in response.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            Debug.WriteLine($"Transcript: { alternative.Transcript}");

                            Application.Current.Dispatcher.Invoke(() =>       // UI 스레드에서 텍스트 박스 업데이트 하기 위한 Dispatcher 사용 
                            {
                                if (microphoneIndex == 1)
                                {
                                    textbox1!.Text += alternative.Transcript + "\\n";   // 인식된 텍스트를 첫 번째 텍스트 박스에 업데이트 
                                }
                                else
                                {
                                    textbox2!.Text += alternative.Transcript + "\\n";   // 인식된 텍스트를 두 번째 텍스트 박스에 업데이트 
                                }
                            });
                        }
                    }
                }

                Debug.WriteLine($"마이크 {microphoneIndex} 스트리밍 완료");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"마이크 {microphoneIndex} 스트리밍 실패: " + ex.Message);
                if (microphoneIndex == 1)
                {
                    streamingCall1 = null;
                }
                else
                {
                    streamingCall2 = null;
                }
                StartRecording(microphoneIndex);
            }
        }

    }

}


