using AILIB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AItraining
{
    /// <summary>
    /// JOIN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JOIN : Page
    {
        //string serverIp = "10.10.21.115";  // 서버 ip설정
        //int serverPort = 12345;            // 포트번호
        //public NetworkStream JOINstream;
        public JOIN()
        {
            InitializeComponent();

            //JOINstream = AIuser.connection(serverIp, serverPort);
            JObject TRegit;
            TRegit = AIuser.signin();       //"JSON"프로토콜 전송
            AIuser.Write(START.stream, TRegit);
        }
        public static JObject RegitObject(string id, string password, string password_check, string per_num)
        {

            JObject jObject = new JObject();

            jObject["id"] = id;
            jObject["pw"] = password;
            jObject["pw_check"] = password_check;
            jObject["person"] = int.Parse(per_num);
            return jObject;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            JObject Regit;
            Regit = JOIN.RegitObject(ID_BOX.Text, PW_BOX.Text, PW_CHECK.Text, PER_NUM.Text);
            AIuser.Write(START.stream, Regit);                                           

            MessageBox.Show("회원가입이 완료되었습니다.");
            START.stream.Close();

            NavigationService.Navigate(new Uri("/START.xaml", UriKind.Relative));
        }
    }
}
