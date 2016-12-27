using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DropNet;
using HeyRed.MarkdownSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SinobigamiBot
{
    public class Dropbox
    {
        DropNetClient Client;
        SettingData setting;

        public Dropbox(SettingData _setting)
        {
            setting = _setting;
            var appKey = setting.Data["DropAppKey"];
            var appSecret = setting.Data["DropAppSecret"];
            if (setting.Data.Keys.Contains("DropUserToken") && setting.Data["DropUserToken"] != ""
                && setting.Data.Keys.Contains("DropUserSecret") && setting.Data["DropUserSecret"] != "")
            {
                var userToken = setting.Data["DropUserToken"];
                var userSecret = setting.Data["DropUserSecret"];

                Client = new DropNetClient(appKey, appSecret, userToken, userSecret);
            }
            else
            {
                Client = new DropNetClient(appKey, appSecret);
                Client.GetToken();
                Console.WriteLine("以下のURLにアクセスしてDropboxへのアクセスを許可してください。許可したらEnterを押してください。");
                Console.WriteLine(Client.BuildAuthorizeUrl());

                var temp = Console.ReadLine();
                var accessToken = Client.GetAccessToken();
                if (accessToken == null)
                {
                    Console.WriteLine("認証エラー");
                    return;
                }
                setting.SetData("DropUserToken", accessToken.Token);
                setting.SetData("DropUserSecret", accessToken.Secret);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>シェアURL</returns>
        public string UploadFile(string filePath)
        {
            using (var fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
            {
                var uploaded = Client.UploadFile("/Public", System.IO.Path.GetFileName(filePath), fs);
                var shareResponse = Client.GetShare($"/Public/{System.IO.Path.GetFileName(filePath)}");
                var url = shareResponse.Url;
                return url;
            }
        }

        public void SaveMarkDown(string markdown)
        {
            var mark = new Markdown();
            var text = mark.Transform(markdown);
            var sw = new System.IO.StreamWriter("markdown.html");
            sw.WriteLine(text);
            sw.Close();
            var url = UploadFile("markdown.html");
            var chrome = new ChromeDriver();
            chrome.Navigate().GoToUrl("./markdown.html");
            var ss = chrome.GetScreenshot();
            ss.SaveAsFile("./markdown.png", System.Drawing.Imaging.ImageFormat.Png);
            chrome.Close();
        }
    }
}
