using System;
using System.Windows;

namespace HearingloopKioskApp.Windows.Conversation
{
    public partial class ConversationZone : Window
    {
        private Scripts.SpeechToText speechToText;

        public ConversationZone()
        {
            InitializeComponent();
            speechToText = new Scripts.SpeechToText();
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
