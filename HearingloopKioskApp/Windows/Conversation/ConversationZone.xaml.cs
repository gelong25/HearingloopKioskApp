﻿using HearingloopKioskApp.Windows.Office;
using System;
using System.Windows;

namespace HearingloopKioskApp.Windows.Conversation
{
    public partial class ConversationZone : Window
    {
        public Scripts.SpeechToText speechToText;

        public ConversationZone()
        {
            InitializeComponent();
            speechToText = new Scripts.SpeechToText
            {
                textbox1 = TextBox1,
                textbox2 = TextBox2
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            speechToText.OnEnable();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            speechToText.OnDisable();
        }

        // ConversationButtonClick 이벤트 핸들러 
        private void ConversationButtonClick(object sender, RoutedEventArgs e)
        {
            MainWindow1 mainWindow = new();
            mainWindow.Show();
            
            this.Close();
        }
    }
}
