using AILIB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
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
    /// AISEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AISEARCH : Page
    {
        public static string gender;
        public static string age;
        public static string phone;
        public static string tenure;
        public static string streaming;
        public static string unlimited;
        public static string charge;
        public static string satisfaction;

        public AISEARCH()
        {
            InitializeComponent();
            GENDER.SelectedIndex = 0;
            PHONE.SelectedIndex = 0;
            STREAMING.SelectedIndex = 0;
            UNLIMITED.SelectedIndex = 0;
            SATISFACTION.SelectedIndex = 0;

            // Static 변수 gender를 초기화합니다.
            gender = GENDER.Text;
            //age = AGE.Text;
            //tenure = TENURE.Text;
            phone = PHONE.Text;
            streaming = STREAMING.Text;
            unlimited = UNLIMITED.Text;
            //charge = CHARGE.Text;
            satisfaction = SATISFACTION.Text;

            //age = AGE.Text;
            //tenure = TENURE.Text;
            //charge = CHARGE.Text;
        }

        private void GENDER_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            GENDER.Text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            // Static 변수 gender를 업데이트합니다.
            gender = GENDER.Text;
        }

        private void PHONE_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            PHONE.Text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            phone = PHONE.Text;
        }

        private void UNLIMITED_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            UNLIMITED.Text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            unlimited = UNLIMITED.Text;
        }

        private void STREAMING_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            STREAMING.Text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            streaming = STREAMING.Text;
        }

        private void SATISFACTION_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            SATISFACTION.Text = (comboBox.SelectedItem as ComboBoxItem).Content.ToString();
            satisfaction = SATISFACTION.Text;
        }

        public static JObject RegitObject(string GENDER, string AGE, string TENURE, string PHONE, string STREAMING, string UNLIMITED, string CHARGE, string SATISFACTION)
        {
            int genderValue = GENDER == "남" ? 0 : 1;

            int phoneValue = PHONE == "휴대폰 개통" ? 0 : 1;

            int streamingValue = STREAMING == "스트리밍 구독" ? 0 : 1;

            int unlimitedValue = UNLIMITED == "무제한 등록" ? 0 : 1;

            int satisfactionValue = 0;

            if (SATISFACTION == "만족도 1")
                satisfactionValue = 1;
            else if (SATISFACTION == "만족도 2")
                satisfactionValue = 2;
            else if (SATISFACTION == "만족도 3")
                satisfactionValue = 3;
            else if (SATISFACTION == "만족도 4")
                satisfactionValue = 4;
            else if (SATISFACTION == "만족도 5")
                satisfactionValue = 5;

            JObject jObject = new JObject();
            jObject["GENDER"] = genderValue;
            jObject["AGE"] = int.Parse(AGE);
            jObject["TENURE"] = int.Parse(TENURE);
            jObject["PHONE"] = phoneValue;
            jObject["STREAMING"] = streamingValue;
            jObject["UNLIMITED"] = unlimitedValue;
            jObject["CHARGE"] = int.Parse(CHARGE);
            jObject["SATISFACTION"] = satisfactionValue;

            return jObject;
        }

        private void SEARCH_BTN_Click(object sender, RoutedEventArgs e)
        {
            JObject search_info;
            search_info = AISEARCH.RegitObject(GENDER.Text, AGE.Text, TENURE.Text, PHONE.Text, STREAMING.Text, UNLIMITED.Text, CHARGE.Text, SATISFACTION.Text);
            AIuser.Write(START.stream, search_info);
            Debug.WriteLine(search_info.ToString());
            age = AGE.Text;
            tenure = TENURE.Text;
            charge = CHARGE.Text;

            NavigationService.Navigate(new Uri("/RESULT.xaml", UriKind.Relative));
        }
    }
}
