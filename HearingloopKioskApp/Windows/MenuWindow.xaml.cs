using HearingloopKioskApp.Windows.Conversation;
using HearingloopKioskApp.Windows.Office;
using HearingloopKioskApp.Windows.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HearingloopKioskApp.Windows
{
    /// <summary>
    /// MenuWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();

        }

        // ToConversationButton 이벤트 핸들러
        private void ToConversationButton(object sender, RoutedEventArgs e)
        {
            ConversationZone conversationZone = new();
            conversationZone.Show();

            this.Close();
        }

        // ToTranslateButton 이벤트 핸들러
        private void ToTranslateButton(object sender, RoutedEventArgs e)
        {
            TranslateZone translateZone = new();
            translateZone.Show();

            this.Close();
        }

        // ToChatGPTButton 이벤트 핸들러
        private void ToChatGPTButton(object sender, RoutedEventArgs e)
        {
            ChatGPTZone chatGPTZone = new();
            chatGPTZone.Show();

            this.Close();
        }

        // ToOfficeButton 이벤트 핸들러
        private void ToOfficeButton(object sender, RoutedEventArgs e)
        {
            OfficeZone officeZone = new();
            officeZone.Show();

            this.Close();
        }

    }




}
