using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using AILIB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AItraining
{
    public partial class START : Page
    {
        string serverIp = "10.10.21.115";  // 서버 ip설정
        int serverPort = 12344;            // 포트번호
        public static NetworkStream stream;
        public static bool login_check = false;
        
        public START()
        {
            InitializeComponent();
            stream = AIuser.connection(serverIp, serverPort);
            JObject first_msg = new JObject();
            first_msg["first_msg"] = "10";
            AIuser.Write(START.stream, first_msg);
        }

        private void LOGIN_BTN_Click(object sender, RoutedEventArgs e)
        {
            JObject login;
            login = AIuser.Login(ID_BOX.Text, PW_BOX.Password);
            AIuser.Write(START.stream, login);

            string response = AIuser.Read(START.stream);
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);
            bool login_bool = jsonResponse.login_success;
            if (login_bool)
            {
                START.login_check = true;
                NavigationService.Navigate(new Uri("/AISEARCH.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("로그인 실패");
            }
        }

        private void join_btn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/JOIN.xaml", UriKind.Relative));
        }
    }
}
