using System;
using System.Windows;

namespace HearingloopKioskApp
{
    public partial class MainWindow : Window
    {
        private SpeechToText speechToText;

        public MainWindow()
        {
            InitializeComponent();
            speechToText = new SpeechToText();
            speechToText.textbox1 = TextBox1;
            speechToText.textbox2 = TextBox2;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            speechToText.OnEnable();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            speechToText.OnDisable();
        }
    }
}
