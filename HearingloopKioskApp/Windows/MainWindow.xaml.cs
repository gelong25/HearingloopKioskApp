using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HearingloopKioskApp.Windows
{

    public partial class MainWindow1 : Window
    {

        private DispatcherTimer timer;
        private int currentImageIndex = 0;

        public MainWindow1()
        {
            InitializeComponent();

            // 타이머 초기화 및 설정 
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();


        }

        // Timer_Tick 이벤트 핸들러 
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // 모든 이미지 Hidden 설정 
            Image1.Visibility = Visibility.Hidden;
            Image2.Visibility = Visibility.Hidden;
            Image3.Visibility = Visibility.Hidden;

            // 현재 인덱스에 해당하는 이미지 Visible 설정 
            switch (currentImageIndex)
            {
                case 0:
                    Image1.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Image2.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Image3.Visibility = Visibility.Visible;
                    break;
            }

            // 다음 인덱스로 이동 (0, 1, 2 순환)
            currentImageIndex = (currentImageIndex + 1) % 3;
        }


        // MainButtonClick 이벤트 핸들러  
        private void MainButtonClick(object? sender, RoutedEventArgs e)
        {
            MenuWindow? menuWindow = new();
            menuWindow.Show();

            this.Close();
        }
    }
}
