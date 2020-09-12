using HealthMachine;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace HealthChecker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private HtmlDocument document = new HtmlDocument();
        private static string domain = "https://eduro.ice.go.kr";
        private static Dictionary<string, string> body = new Dictionary<string, string>
        {
            {"schulNm","" }
        };

        public bool didDiagnosis = false;
        public DateTime didDiagnosisTime;

        public MainWindow()
        {
            InitializeComponent();

            NativeMethods.PreventSleep();

            Task.Factory.StartNew(TimeCheck);
        }

        private async void TimeCheck()
        {
            while (true)
            {
                if (DateTime.Now.Hour == 7 && !didDiagnosis)
                {
                    didDiagnosis = true;
                    didDiagnosisTime = DateTime.Now;

                    await Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
                    {
                        await DoSelfDiagnosis();
                    }));

                }
                else if (DateTime.Now.Hour == 0)
                    didDiagnosis = false;

                Thread.Sleep(60 * 1000);
            }
            
        }
        private HttpContent GetContentForm(Dictionary<string,string> body)
        {
            return new FormUrlEncodedContent(body);
        }
        private bool getResultCodeSucceed(JToken resultSVO)
        {
            return resultSVO["rtnRsltCode"].ToString() == "SUCCESS" ? true : false;
        }
        private void addLog(string log)
        {
            LogList.Items.Add(log);
        }
        private void addLogLine()
        {
            LogList.Items.Add("-------------------");
        }
        private async Task<JObject> postFormAndGetJson(string detailUrl, Dictionary<string,string> form)
        {
            var response = await client.PostAsync($"{domain}/{detailUrl}", GetContentForm(form));
            return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
        }
        private async Task<string> postFormAndGetHTML(string detailUrl, Dictionary<string,string> form)
        {
            var response = await client.PostAsync($"{domain}/{detailUrl}", GetContentForm(form));
            return await response.Content.ReadAsStringAsync();
        }
        //자가 진단 수동 클릭
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SchoolName.Text == "" || StudentName.Text == "" || Birthday.Text == "")
            {
                MessageBox.Show("정보 빈 칸을 채워주세요.");
                return;
            }
            addLog(DateTime.Now.ToString("MM월 dd일 hh시 mm분"));

            await DoSelfDiagnosis();

            addLogLine();
        }

        private async Task DoSelfDiagnosis()
        {
            body["schulNm"] = SchoolName.Text;

            var resultSVO = (await postFormAndGetJson("stv_cvd_co00_004.do", body))["resultSVO"];

            if (getResultCodeSucceed(resultSVO))
            {
                addLog("학교 정보 받기 완료");

                var schulCode = resultSVO["schulCode"].ToString();
                body = new Dictionary<string, string>
                {
                    { "qstnCrtfcNoEncpt","" },
                    { "rtnRsltCode","" },
                    { "schulCode",schulCode },
                    { "schulNm", SchoolName.Text },
                    { "pName",StudentName.Text },
                    { "frnoRidno",Birthday.Text },
                    {"aditCrtfcNo","" }
                };

                resultSVO = (await postFormAndGetJson("stv_cvd_co00_012.do", body))["resultSVO"];
                if (getResultCodeSucceed(resultSVO))
                {
                    addLog("개인 정보 교환 완료");
                    var qstnCrtfcNoEncpt = resultSVO["qstnCrtfcNoEncpt"].ToString();

                    body["qstnCrtfcNoEncpt"] = qstnCrtfcNoEncpt;

                    document.LoadHtml(await postFormAndGetHTML("stv_cvd_co00_000.do", body));

                    var form = document.GetElementbyId("infoForm");

                    foreach (var table in form.SelectNodes("//table"))
                    {
                        var inputElement = table.SelectSingleNode(".//label").SelectSingleNode("./input");
                        body.Add(inputElement.GetAttributeValue("name", ""), inputElement.GetAttributeValue("value", ""));
                    }

                    resultSVO = (await postFormAndGetJson("stv_cvd_co01_000.do", body))["resultSVO"];
                    if (getResultCodeSucceed(resultSVO))
                    {
                        addLog("설문 제출 완료");
                        document.LoadHtml(await postFormAndGetHTML("stv_cvd_co02_000.do", body));
                        //var resultMsg = document.GetElementbyId("content_detail1").SelectSingleNode(".//p").InnerText;

                        addLog("출석 완료되었습니다");
                        DidSelfDiagnosis.Text = "출석 완료";
                        didDiagnosis = true;
                    }
                    else
                    {
                        addLog("잘못된 설문 체킹, 실패");
                    }
                }
                else
                {
                    addLog("잘못된 정보, 교환 실패");
                }
            }
            else
            {
                addLog("학교 정보를 받는데에 실패하였습니다.");
            }

            LogList.SelectedIndex = LogList.Items.Count - 1;
            LogList.ScrollIntoView(LogList.SelectedItem);
        }
    }
}
