using HearingloopKioskApp.Windows.Conversation;
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

        //ButtonClick 이벤트 핸들러 추가 
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            ConversationZone conversationZone = new ConversationZone();
            conversationZone.Show();

            this.Close();
        }

    }




}
