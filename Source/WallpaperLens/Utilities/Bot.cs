using ImageProcessor;
using ImageProcessor.Imaging;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WallpaperLens.Events;

namespace WallpaperLens.Utilities
{
    #region Events

    public class NewTrendEvent : PubSubEvent<string> { }

    #endregion

    public class Bot
    {
        private readonly IEventAggregator _eventAggregator;
        private static Scheduler _scheduler;
        private string _currentUrl;

        public Bot(
            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _scheduler = new Scheduler();
        }

        public void Run()
        {
            _scheduler.Run(CheckReddit, new TimeSpan(1, 0, 0));
        }

        private void CheckReddit()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(@"https://www.reddit.com/api/v1/access_token");
            client.Authenticator = new HttpBasicAuthenticator("PT-Z2_pzNS2ERw", "H7Szdv_a6dTVHsLwvfK2aaoDBUA");


            var request = new RestRequest(Method.POST);
            request.AddHeader("User-Agent", "CSharpScript/0.1 by looterwar");
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", "wallthrow123456");
            request.AddParameter("password", "botmustrun1234");

            var response = client.Execute<AuthResponse>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return;

            Debug.WriteLine(response.Data.access_token);

            var client_oauth = new RestClient();
            client_oauth.BaseUrl = new Uri(@"https://oauth.reddit.com/r/wallpapers");

            var request_oauth = new RestRequest(Method.GET);
            request_oauth.AddHeader("Authorization", string.Format("bearer {0}", response.Data.access_token));
            //request_oauth.AddHeader("User-Agent", "CSharpScript/0.1 by looterwar");

            var response_oauth = client_oauth.Execute<PaperResponse>(request_oauth);

            if (response_oauth.StatusCode != System.Net.HttpStatusCode.OK)
                return;

            var toExclude = new char[] { '{', '}', '(', ')', '[', ']' };
            Match match;

            if (toExclude.Any(x => response_oauth.Data.data.children[0].data.title.Contains(x)))
            {
                var reg = new Regex(@"[^\[\]{}()]+(?![^\[]*\])(?![^{]*})(?![^\(]*\))");
                match = reg.Match(response_oauth.Data.data.children[0].data.title);
            }
            else
            {
                var reg = new Regex(@"[^\dx×]+(?![^\d{4}x×]+\d{4})");
                match = reg.Match(response_oauth.Data.data.children[0].data.title);
            }

            //Debug.WriteLine(match.Value);
            Debug.WriteLine(response_oauth.Data.data.children[0].data.url);

            var targetURL = response_oauth.Data.data.children[0].data.url;

            if (targetURL != _currentUrl)
            {

                var client_download = new RestClient();
                client_download.BaseUrl = new Uri(response_oauth.Data.data.children[0].data.url);

                var request_download = new RestRequest(Method.GET);
                var rawData = client_download.DownloadData(request_download);

                var newSize = new Size((int)System.Windows.SystemParameters.PrimaryScreenWidth, (int)System.Windows.SystemParameters.PrimaryScreenHeight);
                using (MemoryStream inStream = new MemoryStream(rawData))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                        {
                            // Load, resize, set the format and quality and save an image.
                            var layer = new ResizeLayer(newSize, ResizeMode.Stretch, upscale: false);
                            imageFactory.Load(inStream)
                                        .Resize(layer)
                                        //.Format(format)
                                        .Save(outStream);
                        }
                        // Do something with the stream.
                        File.WriteAllBytes(@"D:\bg.png", outStream.ToArray());
                    }
                }

                _currentUrl = targetURL;

                _eventAggregator.GetEvent<NewTrendEvent>().Publish(match.Value);
            }
        }
    }

    public class Post
    {
        public string url { get; set; }
        public bool over_18 { get; set; }
        public string title { get; set; }
    }

    public class Node
    {
        public List<Node> children { get; set; }
        public Post data { get; set; }
    }

    public class PaperResponse
    {
        public Node data { get; set; }
    }

    public class AuthResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }
}
