using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AILIB;
using LiveCharts;
using Newtonsoft.Json.Linq; // Newtonsoft.Json 라이브러리를 사용하여 JSON 파싱

namespace AItraining
{
    public partial class GRP : Page, INotifyPropertyChanged
    {
        private bool _mariaSeriesVisibility;
        private bool _charlesSeriesVisibility;
        private ChartValues<double> _mariaValues;
        private ChartValues<double> _charlesValues;

        public GRP()
        {
            InitializeComponent();

            MariaSeriesVisibility = true;
            CharlesSeriesVisibility = true;

            MariaValues = new ChartValues<double>();
            CharlesValues = new ChartValues<double>();

            DataContext = this;

            DB_data(); // 데이터 로드 메서드 호출
        }

        public bool MariaSeriesVisibility
        {
            get { return _mariaSeriesVisibility; }
            set
            {
                _mariaSeriesVisibility = value;
                OnPropertyChanged(nameof(MariaSeriesVisibility));
            }
        }

        public bool CharlesSeriesVisibility
        {
            get { return _charlesSeriesVisibility; }
            set
            {
                _charlesSeriesVisibility = value;
                OnPropertyChanged(nameof(CharlesSeriesVisibility));
            }
        }

        public ChartValues<double> MariaValues
        {
            get { return _mariaValues; }
            set
            {
                _mariaValues = value;
                OnPropertyChanged(nameof(MariaValues));
            }
        }

        public ChartValues<double> CharlesValues
        {
            get { return _charlesValues; }
            set
            {
                _charlesValues = value;
                OnPropertyChanged(nameof(CharlesValues));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DB_data()
        {
            string response = AIuser.Read(START.stream);
            ParseJsonData(response);
        }

        private void ParseJsonData(string jsonData)
        {
            try
            {
                var json = JObject.Parse(jsonData);
                var testGoArray = json["TEST_GO"] as JArray;

                if (testGoArray != null)
                {
                    MariaValues.Clear();
                    CharlesValues.Clear();

                    foreach (var item in testGoArray)
                    {
                        if (item["STREAM_NO"] != null && item["STREAM_YES"] != null)
                        {
                            if (double.TryParse(item["STREAM_NO"].ToString(), out double streamNo))
                            {
                                MariaValues.Add(streamNo*100);
                            }

                            if (double.TryParse(item["STREAM_YES"].ToString(), out double streamYes))
                            {
                                CharlesValues.Add(streamYes*100);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing JSON data: {ex.Message}");
            }
        }
    }
}
