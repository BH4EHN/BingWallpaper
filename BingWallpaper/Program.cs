using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BingWallpaper
{
    public class Program
    {
        static void Main(string[] args)
        {
            var wallpapaer = new WallPaper();
            var regions = new List<String>
            {
                // WallPaper.Region.DeDe,
                // WallPaper.Region.EnAu,
                // WallPaper.Region.EnCa,
                // WallPaper.Region.EnNz,
                // WallPaper.Region.EnUk,
                // WallPaper.Region.EnUs,
                // WallPaper.Region.JaJp,
                WallPaper.Region.ZhCn
            };
            var resolutions = new List<String>
            {
                WallPaper.Resolution.FhdLandscape,
                WallPaper.Resolution.FhdPortrait,
                WallPaper.Resolution.HdLandScape,
                WallPaper.Resolution.HdPortrait
            };
            foreach (var region in regions)
            {
                foreach (var resolution in resolutions)
                {
                    wallpapaer.FetchImage(region, resolution);
                }
            }
            Console.Write("Download complete, press any key to exit.");
            Console.ReadKey(true);
        }

        public class WallPaper
        {
            public static class Region
            {
                public const String ZhCn = "zh-CN";
                public const String EnUs = "en-US";
                public const String JaJp = "ja-JP";
                public const String EnAu = "en-AU";
                public const String EnUk = "en-UK";
                public const String DeDe = "de-DE";
                public const String EnNz = "en-NZ";
                public const String EnCa = "en-CA";
            }

            public static class Resolution
            {
                public const String HdLandScape = "1366x768";
                public const String HdPortrait = "768x1366";
                public const String FhdLandscape = "1920x1080";
                public const String FhdPortrait = "1080x1920";
            }

            private const String DownloadDir = "download/";

            public WallPaper()
            {
                if (!Directory.Exists(DownloadDir))
                {
                    Directory.CreateDirectory(DownloadDir);
                }
            }

            public Boolean FetchImage(String region, String resolution)
            {
                const String bingBaseUrl = "http://www.bing.com";

                var webClient = new WebClient();
                var url = String.Format("http://www.bing.com/HPImageArchive.aspx?format=js&idx={0}&n={1}&mkt={2}", 0, 1, region);
                var data = webClient.DownloadData(url);
                var json = Encoding.UTF8.GetString(data);
                var wallpaperInfo = Json.Parse<WallPaperInfo>(json);

                String dateDir = DateTime.Now.ToString("yyyyMMdd") + "/";
                if (!Directory.Exists(DownloadDir + dateDir))
                {
                    Directory.CreateDirectory(DownloadDir + dateDir);
                }
                using (var writer = new StreamWriter(DownloadDir + dateDir + region + ".xml", false)) 
                {
                    (new XmlSerializer(typeof(WallPaperInfo))).Serialize(writer.BaseStream, wallpaperInfo);
                }
                    


                var imageUrl = String.Format("{0}{1}_{2}.{3}", bingBaseUrl, wallpaperInfo.Image[0].UrlBase, resolution, "jpg");
                var filename = DownloadDir + dateDir + GetFileNameFromUrl(imageUrl);
                try
                {
                    webClient.DownloadFile(imageUrl, filename);
                    Console.WriteLine("{0} {1} fetched in {2}", region, resolution, filename);
                }
                catch (WebException exception)
                {
                    Console.WriteLine("[Error] {0} {1}, Reason: {2}", region, resolution, exception);
                }
                return true;
            }

            private static string GetFileNameFromUrl(String url)
            {
                return System.IO.Path.GetFileName(url);
            }

            static class Json
            {
                public static T Parse<T>(string jsonString)
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                    {
                        return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
                    }
                }

                public static string Stringify(object jsonObject)
                {
                    using (var ms = new MemoryStream())
                    {
                        new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }

            [DataContract]
            public class WallPaperInfo
            {
                [DataContract]
                public class ImageItem
                {
                    [DataContract]
                    public class HsItem
                    {
                        [DataMember(Name = "desc")]
                        public String Description;
                        [DataMember(Name = "link")]
                        public String Link;
                        [DataMember(Name = "query")]
                        public String Query;
                        [DataMember(Name = "locx")]
                        public String LocX;
                        [DataMember(Name = "locy")]
                        public String LocY;
                    }

                    [DataContract]
                    public class MsgItem
                    {
                        [DataMember(Name = "title")]
                        public String Title;
                        [DataMember(Name = "link")]
                        public String Link;
                        [DataMember(Name = "text")]
                        public String Text;
                    }

                    [DataMember(Name = "startdate")]
                    public String StartDate;
                    [DataMember(Name = "fullstartdate")]
                    public String FullStartDate;
                    [DataMember(Name = "enddate")]
                    public String EndDate;
                    [DataMember(Name = "url")]
                    public String Url;
                    [DataMember(Name = "urlbase")]
                    public String UrlBase;
                    [DataMember(Name = "copyright")]
                    public String CopyRight;
                    [DataMember(Name = "copyrightlink")]
                    public String CopyRightLink;
                    [DataMember(Name = "wp")]
                    public Boolean Wp;
                    [DataMember(Name = "hsh")]
                    public String Hash;
                    [DataMember(Name = "drk")]
                    public Int32 Drk;
                    [DataMember(Name = "top")]
                    public Int32 Top;
                    [DataMember(Name = "bot")]
                    public Int32 Bot;
                    [DataMember(Name = "hs")]
                    public HsItem[] Hs;
                    [DataMember(Name = "msg")]
                    public MsgItem[] Msg;
                }

                [DataContract]
                public class ToolTip
                {
                    [DataMember(Name = "loading")]
                    public String Loading;
                    [DataMember(Name = "previous")]
                    public String Previous;
                    [DataMember(Name = "next")]
                    public String Next;
                    [DataMember(Name = "walle")]
                    public String Walle;
                    [DataMember(Name = "walls")]
                    public String Walls;
                }

                [DataMember(Name = "images")]
                public ImageItem[] Image;
                [DataMember(Name = "tooltips")]
                public ToolTip Tooltip;
            }

        }
    }
}
