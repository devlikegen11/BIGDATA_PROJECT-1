using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AItraining
{
    public partial class RESULT : Page
    {
        public RESULT()
        {
            InitializeComponent();
            string response = AILIB.AIuser.Read(START.stream);
            JObject jsonResponse = JObject.Parse(response);

            double noStreamValue = (double)jsonResponse["no_stream"];
            double withStreamValue = (double)jsonResponse["with_stream"];
            UpdateUI(noStreamValue, withStreamValue);
        }

        public void UpdateUI(double n_str, double y_str)
        {
            NO_STREAM.Text = n_str.ToString("P0");
            WITH_STREAM.Text = y_str.ToString("P0");
            string gend = AISEARCH.gender;
            string age = AISEARCH.age;
            string tenure = AISEARCH.tenure;
            string phone = AISEARCH.phone;
            string streaming = AISEARCH.streaming;
            string unlimited = AISEARCH.unlimited;
            string charge = AISEARCH.charge;
            string satisfaction = AISEARCH.satisfaction;
            test.Text = gend +"/ "+ age + "세/ " + tenure + "개월/ " + phone + "/ \n" + streaming + "/ " + unlimited + "/ " + charge+"000원/ " + satisfaction + "점";
        }

        private void GRP_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GRP.xaml", UriKind.Relative));
        }
    }
}
