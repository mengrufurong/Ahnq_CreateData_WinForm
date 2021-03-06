﻿using NewLife.Xml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace Ahnq
{
    public partial class Form1 : Form
    {
        public static int countNumber = 0;
        public static int errorNumber = 0;
        public Form1()
        {
            InitializeComponent();
        }
        //public string LogMessage
        //{
        //    get { return listViewDataLog.SelectedItems.ToString(); }
        //    set { listViewDataLog.Items.Add(value); }
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            textBoxFarmSerialnum.Text = AppConfig.Current.Serialnum;
            textBoxFacilityCount.Text = AppConfig.Current.DeviceCountPerFacility.ToString();
            textBoxUrl.Text = AppConfig.Current.RequestUrl.ToString();
            textBoxStressTestPerTimes.Text = AppConfig.Current.TimeInterval.ToString();

            foreach (Facility fac in AppConfig.Current.Facilities)
            {
                string[] subitem = new[] { fac.Serialnum, fac.Name, fac.FacilityTypeSerialnum };
                var item = new ListViewItem(subitem);
                item.Tag = fac;
                listView1.Items.Add(item);
            }

            String[] facilityTypeArr = new String[] { "livestock-YaShe", "livestock-JiShe", "datian-XiaoMai", "aquatic-Yu", "aquatic-Xie", "livestock-YangShe", "aquatic-Xia", "bird", "datian-ShuiDao", "datian-ChaYe", "livestock", "livestock-ZhuShe", "wenshi", "datian", "wenshi-ShuCai", "wenshi-HuaHui", "aquatic" };
            for (int i = 0; i < facilityTypeArr.Length; i++)
            {
                comboBox1.Items.Add(facilityTypeArr[i]);
            }
            //下面两种方法都可以为ComboBox赋初试选中值
            //comboBox1.SelectedIndex = 0;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var str = textBoxFarmSerialnum.Text.Trim();
            AppConfig.Current.Serialnum = str;
            AppConfig.Current.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string urlString = AppConfig.Current.RequestUrl;
            string postDataInfoUrl = "http://" + urlString + "/api/FarmApi/UploadFacility";
            var fac = new Facility();
            int tempNum = 0;
            //fac.FacilityName = textBoxFarmSerialnum.Text.Trim();
            foreach (var temp in AppConfig.Current.Facilities)
            {
                var facilitySerialnumTemp = temp.Serialnum.Substring(10, 3).ToInt();
                if (facilitySerialnumTemp > tempNum)
                {
                    tempNum = facilitySerialnumTemp;
                }

            }
            tempNum++;
            fac.Serialnum = textBoxFarmSerialnum.Text.Trim() + "G" + (tempNum / 100).ToString() + ((tempNum / 10) % 10) + (tempNum % 10).ToString();
            fac.Name = textBoxFacilityName.Text.Trim();
            fac.FacilityTypeSerialnum = comboBox1.SelectedItem.ToString();
            fac.FarmSerialnum = AppConfig.Current.Serialnum;
            //设施类型ID
            //fac.FacilityTypeSerialnum=
            //将新添加的设施区域添加到数据库中
            var requestJson = JsonConvert.SerializeObject(fac);
            HttpContent httpContent = new StringContent(requestJson);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var httpClient = new HttpClient();
            var responseJson = httpClient.PostAsync(postDataInfoUrl, httpContent).Result.Content.ReadAsStringAsync().Result;

            textBoxDataLog.AppendText($"{DateTime.Now:hh:mm:ss tt zz}");
            textBoxDataLog.AppendText(responseJson);
            textBoxDataLog.AppendText("\r\n");

            AppConfig.Current.Facilities.Add(fac);
            AppConfig.Current.Save();

            string[] subitem = new[] { fac.Serialnum, fac.Name, fac.FacilityTypeSerialnum };
            var item = new ListViewItem(subitem);
            item.Tag = fac;
            listView1.Items.Add(item);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            var str = textBoxFacilityCount.Text.Trim();
            var count = 5;
            if (int.TryParse(str, out count))
            {
                AppConfig.Current.DeviceCountPerFacility = count;
                AppConfig.Current.Save();
            }
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteFacility();
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFacility();
        }

        void DeleteFacility()
        {
            var items = listView1.SelectedItems;
            if (items.Count > 0)
            {
                foreach (ListViewItem item in items)
                {
                    var facility = item.Tag as Facility;
                    AppConfig.Current.Facilities.RemoveAll(e => e.Serialnum.Equals(facility.Serialnum));

                    listView1.Items.Remove(item);
                }
                AppConfig.Current.Save();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBoxFacilityName_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxFacilityType_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxFacilitySerialnum_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxStressTestPerTimes_TextChanged(object sender, EventArgs e)
        {
            var timeNum = textBoxStressTestPerTimes.Text.Trim();
            int count = 5;
            if (int.TryParse(timeNum, out count))
            {
                AppConfig.Current.TimeInterval = count;
                AppConfig.Current.Save();
            }
        }



        void Method(object source, System.Timers.ElapsedEventArgs e)
        {
            if(textBoxDataLog.TextLength>0xffff)
                textBoxDataLog.Clear();

            string urlString = AppConfig.Current.RequestUrl;
            string postDataValueUrl = "http://" + urlString + "/api/FarmApi/UploadDeviceData";
            Random rd = new Random();
            var facilities = AppConfig.Current.Facilities;
            for (var i = 0; i < facilities.Count(); i++)
            {
                var facility = facilities.ElementAt(i);
                var devices = facility.Devices;
                foreach (var device in devices)
                {
                    var deviceValue = new DeviceValue();
                    deviceValue.Serialnum = device.Serialnum;
                    deviceValue.ShowValue = rd.Next(0, 100).ToString();
                    deviceValue.ProcessedValue = rd.Next(0, 100);
                    deviceValue.UpdateTime = DateTime.Now;
                    deviceValue.IsException = true;

                    var requestJson = JsonConvert.SerializeObject(deviceValue);
                    HttpContent httpContent = new StringContent(requestJson);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var httpClient = new HttpClient();
                    countNumber++;
                    labelMsg.Text = "运行中。。。请求次数：" + countNumber + "次！失败次数：" + errorNumber;
                    try
                    {
                        var responseJson = httpClient.PostAsync(postDataValueUrl, httpContent).Result.Content.ReadAsStringAsync().Result;
                        textBoxDataLog.Text += $"{DateTime.Now:hh:mm:ss tt zz}{responseJson.ToString()}" + "-当前第" +
                                               countNumber.ToString() + "次请求完成！\r\n";
                    }
                    catch (Exception ex)
                    {
                        errorNumber++;
                        textBoxDataLog.Text += $"{DateTime.Now:hh:mm:ss tt zz}" + "本次请求失败！" + "-当前第" +
                                               countNumber.ToString() + "次请求！\r\n";
                    }
                    textBoxDataLog.ScrollToCaret();
                }
            }
            //   AppConfig.Current.Save();
        }

        //public void AddMessage(object sender, EventArgs e,string responseJson)
        //{

        //}
        //void Method()
        //{
        //    string postDataValueUrl = "http://loc:10099/api/FarmApi/UploadDeviceData";
        //    Random rd = new Random();
        //    var facilities = AppConfig.Current.Facilities;
        //    for (var i = 0; i < facilities.Count(); i++)
        //    {
        //        var facility = facilities.ElementAt(i);
        //        var devices = facility.Devices;
        //        foreach (var device in devices)
        //        {
        //            var deviceValue = new DeviceValue();
        //            deviceValue.Serialnum = device.Serialnum;
        //            //deviceValue.DeviceSerialnum = GenerateRandomString(10);
        //            deviceValue.ShowValue = rd.Next(0, 100).ToString();
        //            deviceValue.ProcessedValue = rd.Next(0, 100);
        //            deviceValue.UpDateTime = DateTime.Now;
        //            deviceValue.IsException = true;

        //            var requestJson = JsonConvert.SerializeObject(deviceValue);
        //            HttpContent httpContent = new StringContent(requestJson);
        //            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //            var httpClient = new HttpClient();

        //            var responseJson = httpClient.PostAsync(postDataValueUrl, httpContent)
        //       .Result.Content.ReadAsStringAsync().Result;
        //            AppConfig.Current.callbackMessage.Add(responseJson);
        //            //将值添加到设备中
        //            //  device.DeviceValues.Add(deviceValue);

        //        }
        //    }
        //    AppConfig.Current.Save();
        //}

        #region 产生随机字符串 +string GenerateRandomNumber(int Length)
        private static char[] constant = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public static string GenerateRandomString(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
        #endregion

        private System.Timers.Timer timeFunc;
        private void button2_Click(object sender, EventArgs e)
        {
            countNumber = 0;
            errorNumber = 0;

            var time = AppConfig.Current.TimeInterval;
            var timeMin = time * 1000;
            if (timeFunc == null)
            {
                timeFunc = new System.Timers.Timer(timeMin);//实例化Timer类，设置时间间隔为100毫秒
                timeFunc.Elapsed += new System.Timers.ElapsedEventHandler(Method);//当到达时间的时候执行MethodTimer2事件 
                timeFunc.AutoReset = true;//false是执行一次，true是一直执行
            }

            labelMsg.Text = "运行中。。。";
            timeFunc.Enabled = true;//设置是否执行System.Timers.Timer.Elapsed事件 

            Method(null, null);
        }

        private void buttonAddDevice_Click(object sender, EventArgs e)
        {
            string[] deviceSerialnum = new string[] { "collect-qiti-AnQi", "collect-qiti-ErYangHuaTan", "collect-qiti-LiuHuaQing", "collect-qiti-YiYangHuaTan", "collect-qixiang-FengSu", "collect-qixiang-FengXiang", "collect-qixiang-GuangHeYouXiaoFuShe", "collect-qixiang-GuangZhao", "collect-qixiang-HuWaiShiDu", "collect-qixiang-HuWaiWenDu", "collect-qixiang-JiangYuLiang", "collect-qixiang-KongQiWenDu", "collect-qixiang-KongQiXiangDuiShiDu", "collect-qixiang-QiYa", "collect-qixiang-RiZhaoShiShu", "collect-qixiang-YuXueGanZhi", "collect-qixiang-ZhengFa", "collect-shuiti-AnDan", "collect-shuiti-PH", "collect-shuiti-RongJieYang", "collect-shuiti-ShuiWei", "collect-shuiti-ShuiWen", "collect-shuiti-YaXiaoSuanYan", "collect-shuiti-ZhongJinShu", "collect-turang-DianDaoLv", "collect-turang-TuRangShiDu", "collect-turang-TuRangWenDu" };
            var length = deviceSerialnum.Length - 1;
            var facilities = AppConfig.Current.Facilities;
            string urlString = AppConfig.Current.RequestUrl;
            string postDataInfoUrl = "http://" + urlString + "/api/FarmApi/UploadDevice";
            //var s = facilities.Count();
            //var facilitiesTemp = new List<Facility>();
            Random rds = new Random();
            var httpClient = new HttpClient();
            foreach (var facility in facilities)
            {
                //var temp = facilities.ElementAt(m);
                int count = AppConfig.Current.DeviceCountPerFacility;
                var devices = facility.Devices;

                if (devices.Count > count)
                {
                    devices.RemoveRange(count, devices.Count() - count);
                }
                if (devices.Count < count)
                {
                    var startIndex = 0;
                    if (devices.Count > 0)
                    {
                        startIndex = devices.Max(dev => Convert.ToInt32(dev.Serialnum.Replace(facility.Serialnum + "D", "")));
                    }

                    for (int i = startIndex; i < count; i++)
                    {
                        var device = new Device();
                        device.Serialnum = facility.Serialnum + "D" + i.ToString().PadLeft(3, '0');
                        device.DeviceTypeSerialnum = deviceSerialnum[rds.Next(0, length)];
                        device.FacilitySerialnum = facility.Serialnum;
                        device.Name = device.Serialnum;
                        devices.Add(device);

                        //将随机生成的设备信息传到接口中
                        var requestJson = JsonConvert.SerializeObject(device);
                        HttpContent httpContent = new StringContent(requestJson);
                        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var responseJson = httpClient.PostAsync(postDataInfoUrl, httpContent).Result.Content.ReadAsStringAsync().Result;
                        textBoxDataLog.AppendText($"{DateTime.Now:hh:mm:ss tt zz}");
                        textBoxDataLog.AppendText(responseJson);
                        textBoxDataLog.AppendText("\r\n");
                    }
                }


                AppConfig.Current.Save();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            timeFunc.Enabled = false;
            labelMsg.Text = "已停止！";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void labelMsg_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            var tempValue = textBoxUrl.Text.Trim();
            if (!tempValue.IsNullOrEmpty())
            {
                AppConfig.Current.RequestUrl = tempValue;
                AppConfig.Current.Save();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void listViewDataLog_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDataLog_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
