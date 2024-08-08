using HtmlAgilityPack;
using NuGet;
using System;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Compression;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace parserv2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //setting encoding to UTF-16
            Console.OutputEncoding = Encoding.Unicode;

            List<string> GstrURL = new List<string>();
            //sample url:https://e-hentai.org/g/2432194/920515f96f/
            string MANGA_URL = "https://e-hentai.org/g/2432194/920515f96f/";
            string MANGA_img_URL = new List<string>(MANGA_URL.Split('/'))[4];
            string MANGA_Pages_Numbers = string.Empty;
            string MANGA_Title = string.Empty;
            int MANGA_intPages = 0;
            const int MANGA_AverageNumbers = 40;
            int MANGA_Processing_Mark = 0;

            while (true)
            {
                //intput eh target url
                HtmlDocument target_Document;
                inputTarget_URL(out MANGA_URL, out MANGA_img_URL, out target_Document);

                //page numbers process
                processPage_Number(out MANGA_Pages_Numbers, out MANGA_intPages, target_Document);

                //title process
                MANGA_Title = processTitle(target_Document);

                //replace unexpect word
                MANGA_Title = replaceUnexpect_Word(MANGA_Title);

                //add img url to list string
                addTargetImg_URL_toListString(GstrURL, MANGA_URL, MANGA_img_URL, MANGA_intPages, MANGA_AverageNumbers);

                //create file path if path don't exist
                createFile_Path(MANGA_Title);

                //do download imag section
                processListString_toLocal_Imag();
            }

            string MParserimg(List<string> page_urls, int startnumber = 0)
            {
                //make it to processing state
                MANGA_Processing_Mark = 0;
                for (int i = startnumber; i < page_urls.Count; i++)
                {
                    Thread.Sleep(500);
                    //make processing mark = current processong page
                    MANGA_Processing_Mark++;
                    HtmlWeb page_url = new HtmlWeb();
                    // Set the PreRequest handler to add a cookie
                    page_url.PreRequest = request =>
                    {
                        // Create a new cookie container
                        CookieContainer cookieContainer = new CookieContainer();

                        // Add a cookie (change the name and value to match your requirement)
                        cookieContainer.Add(new Cookie("nw", "1", "/", "e-hentai.org"));

                        // Assign the cookie container to the request
                        if (request is HttpWebRequest webRequest)
                        {
                            webRequest.CookieContainer = cookieContainer;
                        }

                        return true; // Indicate that the request should proceed
                    };
                    HtmlDocument target_img_url = page_url.Load(page_urls[i]);
                    HtmlNode target_img = target_img_url.DocumentNode.SelectSingleNode(
                        $"//img[@id='img']");

                    string imageUrl = target_img.GetAttributeValue("src", "");
                    string savePath = @"D:\comic\" + MANGA_Title + @"\ "
                        + Convert.ToString(i + 1) + ".png"; // 設定儲存路徑
                    //download img and output seccess message
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile(imageUrl, savePath);
                        Console.WriteLine("success " + MANGA_Title + " "
                            + Convert.ToString(i + 1));
                    }
                }
                //clear string list
                GstrURL.Clear();
                //if program done turn of processing state
                MANGA_Processing_Mark = -1;
                return MANGA_Title + " _____Done";
            }

            void processListString_toLocal_Imag()
            {
                //do download imag secsion
                try
                {
                    //continue image from number
                    Console.WriteLine("\nPlease input image index");
                    string stringtemp = Console.ReadLine();
                    int pagetemp;
                    //let user can select page
                    if (!int.TryParse(stringtemp, out pagetemp))
                    {
                        pagetemp = 0;
                    }

                    //go url
                    Console.WriteLine(MParserimg(GstrURL, pagetemp));
                }
                finally
                {
                    while (MANGA_Processing_Mark != -1)
                    {
                        //continue image from number
                        Console.WriteLine("\nPlease try again input continue image number");
                        //if input valid number 
                        int intcontinue;
                        try
                        {
                            intcontinue = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (Exception eintcontinue)
                        {
                            //if input worng again go 0 default restart
                            Console.WriteLine(eintcontinue + "\nplease input number or restart on last download page");
                            intcontinue = MANGA_Processing_Mark--;
                        }
                        Console.WriteLine(MParserimg(GstrURL, intcontinue));
                    }
                }
            }


            ////manga img url process
            //// 選擇所有 class 包含 'gdtm' 的 <div> 元素裡的 有包含關鍵番號的<a>
            //HtmlNodeCollection divElements = target_Document.DocumentNode.SelectNodes(
            //    $"//div[contains(@class, 'gdtm')]//a[contains(@href,{MANGA_img_URL})]");
            //// 檢查是否有匹配的 <div> 元素
            //if (divElements != null && divElements.Count > 0)
            //{
            //    foreach (HtmlNode divElement in divElements)
            //    {
            //        //Console.WriteLine(divElement.GetAttributeValue("href", ""));
            //        GstrURL.Add(divElement.GetAttributeValue("href", ""));
            //    }
            //    //測試正常
            //    //foreach(string s in GstrURL)
            //    //Console.WriteLine(s);
            //}


            /*example
            // 選擇所有 class 包含 'gdt' 的 <div> 元素
            HtmlNodeCollection divElements = target_Document.DocumentNode.SelectNodes("//div[contains(@class, 'gdt')]");

            // 檢查是否有匹配的 <div> 元素
            if (divElements != null && divElements.Count > 0)
            {
                // 迭代每個匹配的 <div> 元素
                foreach (HtmlNode divElement in divElements)
                {
                    // 在每個 <div> 元素中篩選具有 'href' 屬性的 <a> 元素
                    HtmlNodeCollection anchorElements = divElement.SelectNodes(".//a[@href]");

                    // 檢查是否有匹配的 <a> 元素
                    if (anchorElements != null && anchorElements.Count > 0)
                    {
                        // 迭代每個匹配的 <a> 元素
                        foreach (HtmlNode anchorElement in anchorElements)
                        {
                            // 取得 <a> 元素的 'href' 屬性值
                            string url = anchorElement.GetAttributeValue("href", "");

                            // 做些其他處理或輸出
                            Console.WriteLine(url);
                        }
                    }
                }
            }
            */


            //if (divElements != null && divElements.Count > 0)
            //{
            //    foreach (HtmlNode divElement in divElements)
            //    {
            //        // 取得 div 元素的內容
            //        string divContent = divElement.InnerHtml;

            //        // 做些其他處理或輸出
            //        Console.WriteLine(divContent);
            //    }
            //}


            //HtmlNodeCollection getURLlink = target_Document.DocumentNode.SelectNodes(
            //    $"/html/body/div[7]/div[1]/div");
            //HtmlNodeCollection getURLlink = target_Document.DocumentNode.SelectNodes(
            //"div:nth-child(7) div:nth-child(1) div a");
            //        HtmlNodeCollection getURLlink = (HtmlNodeCollection)(
            //            target_Document.DocumentNode.Descendants("div")
            //.Where(div => div.GetAttributeValue("class", "") == "gdt"));

            //string t = $"/html/body/div[7]/div[5]/div/a";
            //if (getURLlink != null && getURLlink.Count > 0 )
            //Console.WriteLine(getURLlink.Count());
            //else
            //    Console.WriteLine("non");
            ////Console.WriteLine(getURLlink[0].InnerHtml.ToString());

            //if (getURLlink != null && getURLlink.Count > 0)
            //{
            //    foreach (HtmlNode node in getURLlink)
            //    {
            //        // Get the value of the 'href' attribute
            //        string url = node.GetAttributeValue("href", "");

            //        // Do something with the URL
            //        Console.WriteLine(url);
            //    }
            //}



            //if (getURLlink != null && getURLlink.Count > 0)
            //{
            //    foreach (HtmlNode node in getURLlink)
            //    {
            //        // Select all <a> tags within the current node
            //        HtmlNodeCollection anchorTags = node.SelectNodes(".//a");

            //        if (anchorTags != null)
            //        {
            //            foreach (HtmlNode anchor in anchorTags)
            //            {
            //                // Get the value of the 'href' attribute
            //                string url = anchor.GetAttributeValue("href", "");

            //                // Do something with the URL
            //                Console.WriteLine(url);
            //            }
            //        }
            //    }
            //}

            //    if(getURLlink != null) { 
            //    var a = getURLlink;
            //    Console.WriteLine(a.ToString());
            //}

            //string a = getURLlink[0].Attributes["href"].Value;
            //Console.WriteLine(a);

            //foreach ( var URLelement in getURLlink ) {
            //    Console.WriteLine(URLelement.ToString());

            //}

            //*[@id="gdt"]
            //div gdt
            // / html / body / div[7]
            //div a and img
            // / html / body / div[7] / div[5] / div / a / img
            // /html/body/div[7]/div[1]/div/a

            //target img
            // / html / body / div[1] / div[2] / a / img



            //if (check_getURLlink != null)
            //{

            //    Console.WriteLine(check_getURLlink);
            //    Console.WriteLine("success");
            //}



            //if (getURLimg != null)
            //{
            //    string str1 = getURLstr[0].InnerHtml.ToString();

            //    Console.WriteLine(str1);
            //    Console.WriteLine("success");
            //}
        }

        private static void createFile_Path(string MANGA_Title)
        {
            //create file path if path don't exist
            string folderPath = @"D:\comic\" + MANGA_Title;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        private static void addTargetImg_URL_toListString(List<string> GstrURL, string MANGA_URL, string MANGA_img_URL, int MANGA_intPages, int MANGA_AverageNumbers)
        {
            //add img url to list string
            for (int i = 0; i < (int)(MANGA_intPages / MANGA_AverageNumbers) + 1; i++)
            {
                //p = pages ,nw=no warning and like session 
                string url = MANGA_URL + $"/?p={i}?nw=session";
                HtmlWeb htmlwebtemp = new HtmlWeb();
                // Set the PreRequest handler to add a cookie
                htmlwebtemp.PreRequest = request =>
                {
                    // Create a new cookie container
                    CookieContainer cookieContainer = new CookieContainer();

                    // Add a cookie (change the name and value to match your requirement)
                    cookieContainer.Add(new Cookie("nw", "1", "/", "e-hentai.org"));

                    // Assign the cookie container to the request
                    if (request is HttpWebRequest webRequest)
                    {
                        webRequest.CookieContainer = cookieContainer;
                    }

                    return true; // Indicate that the request should proceed
                };
                HtmlDocument manga_page_document = htmlwebtemp.Load(url);
                HtmlNodeCollection divelements = manga_page_document.DocumentNode.SelectNodes(
                $"//div[contains(@class, 'gdtm')]//a[contains(@href,{MANGA_img_URL})]");
                //add img url to list<string>
                if (divelements != null && divelements.Count > 0)
                {
                    foreach (HtmlNode divelement in divelements)
                    {
                        GstrURL.Add(divelement.GetAttributeValue("href", ""));
                    }
                }
            }
        }

        private static string replaceUnexpect_Word(string MANGA_Title)
        {
            //Console.WriteLine(MANGA_Title);
            //replace unexpect word
            MANGA_Title = MANGA_Title.Replace("\\", "");
            MANGA_Title = MANGA_Title.Replace("/", "");
            MANGA_Title = MANGA_Title.Replace(":", "");
            MANGA_Title = MANGA_Title.Replace("*", "");
            MANGA_Title = MANGA_Title.Replace("?", "");
            MANGA_Title = MANGA_Title.Replace("<", "");
            MANGA_Title = MANGA_Title.Replace(">", "");
            MANGA_Title = MANGA_Title.Replace("|", "");
            MANGA_Title = MANGA_Title.Replace("\"", "");
            MANGA_Title = MANGA_Title.Replace(".", "");
            MANGA_Title = MANGA_Title.Replace("!", "");
            MANGA_Title = MANGA_Title.Replace("~", "");

            //Console.WriteLine(MANGA_Title);
            return MANGA_Title;
        }

        private static string processTitle(HtmlDocument target_Document)
        {
            string MANGA_Title;
            //complite
            //title process
            HtmlNodeCollection getTitle = target_Document.DocumentNode.SelectNodes(
                $"//h1[contains(@id,'gn')]");
            HtmlNode getTitleNode = getTitle[0];
            MANGA_Title = getTitleNode.InnerText;
            return MANGA_Title;
        }

        private static void processPage_Number(out string MANGA_Pages_Numbers, out int MANGA_intPages, HtmlDocument target_Document)
        {
            //complite
            //page numbers process
            HtmlNodeCollection getPagesText = target_Document.DocumentNode.SelectNodes(
                $"//td[contains(@class,'gdt2')]");
            HtmlNode getPagesTextNode = getPagesText[5];
            MANGA_Pages_Numbers = getPagesTextNode.InnerHtml.Substring(
                0, getPagesTextNode.InnerHtml.Length - 6);
            MANGA_intPages = Convert.ToInt32(MANGA_Pages_Numbers);
            //Console.WriteLine(MANGA_intPages);
        }

        private static void inputTarget_URL(out string MANGA_URL, out string MANGA_img_URL, out HtmlDocument target_Document)
        {
            //intput eh target url
            Console.WriteLine("if you want parser eh\nplease input url:" +
                 "https://e-hentai.org/g/2432194/920515f96f/" +
                 "\n---------------------------------");
            MANGA_URL = Console.ReadLine();
            MANGA_img_URL = new List<string>(MANGA_URL.Split('/'))[4];
            //Console.WriteLine(MANGA_img_URL);
            HtmlWeb target_Web_URL = new HtmlWeb();
            // Set the PreRequest handler to add a cookie
            target_Web_URL.PreRequest = request =>
            {
                // Create a new cookie container
                CookieContainer cookieContainer = new CookieContainer();

                // Add a cookie (change the name and value to match your requirement)
                cookieContainer.Add(new Cookie("nw", "1", "/", "e-hentai.org"));

                // Assign the cookie container to the request
                if (request is HttpWebRequest webRequest)
                {
                    webRequest.CookieContainer = cookieContainer;
                }

                return true; // Indicate that the request should proceed
            };
            target_Document = target_Web_URL.Load(MANGA_URL + "?nw=session");
        }
        //reference url
        //https://dotblogs.com.tw/Lance_Blog/2019/03/10/114838


        //parser url process

        //find img page

        //loop download img

        //exception process




    }



}
