using System.Text;

namespace WebServer
{
    public class ResponsePacket
    { 
        public string? Redirect { get; set; }
        public byte[]? Data { get; set; }
        public string? ContentType { get; set; }
        public Encoding? Encoding { get; set; }
    }

    public class Route
    { 
    
    }

    public class ExtensionInfo
    { 
        public string? ContentType { get; set; }
        public Func<string, string, ExtensionInfo, ResponsePacket>? Loader { get; set; }
    }

    public class Router
    {
        public string? WebsitePath { get; set; }
        private Dictionary<string, ExtensionInfo> extFolderMap;

        public Router()
        {
            extFolderMap = new Dictionary<string, ExtensionInfo>()
            {
                {"ico", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico"} },
                {"png", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico"} },
                {"jpg", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico"} },
                {"gif", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico"} },
                {"bmp", new ExtensionInfo() { Loader=ImageLoader, ContentType="image/ico"} },
                {"html", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html"} },
                {"css", new ExtensionInfo() { Loader=FileLoader, ContentType="text/css"} },
                {"js", new ExtensionInfo() { Loader=FileLoader, ContentType="text/javascript"} },
                {"/", new ExtensionInfo() { Loader=PageLoader, ContentType="text/html"} },
            };
        }

        /// <summary>
        /// Read in an Image file and returns a REsponsePacket with the raw data.
        /// </summary>
        private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fStream);
            ResponsePacket ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
            br.Close();
            fStream.Close();

            return ret;
        }

        /// <summary>
        /// Read in what is basically a text file and return a ResponsePacket with the text UTF8 encoded.
        /// </summary>
        private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
        { 
            string text = File.ReadAllText(fullPath);
            ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

            return ret;
        }

        /// <summary>
        /// Load an HTML file, taking into account missing extensions and file/less IP/domain,
        /// which should default to index.html.
        /// </summary>
        private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
        {
            ResponsePacket ret = new();

            if (fullPath == WebsitePath)
                ret = Route("GET", "/index.html", null);
            else
            {
                if (string.IsNullOrEmpty(ext))
                    // No extension, so it is ".html".
                    fullPath = fullPath + ".html";

                // Inject the "Pages" folder into the path
                string remainder = fullPath.Substring(WebsitePath.Length);
                fullPath = WebsitePath + "\\Pages" + remainder;
                ret = FileLoader(fullPath, ext, extInfo);
            }

            return ret;
        }

        /// <summary>
        /// Routes an HTTP request by extracting the file extension from the path and invoking the appropriate loader
        /// to generate a response packet, or returns null if no mapping exists.
        /// </summary>
        public ResponsePacket Route(string verb, string path, Dictionary<string, object> kvParams)
        {
            int index = path.LastIndexOf(".");
            string ext = (index >= 0) ? path.Substring(index + 1) : path;
            ExtensionInfo extInfo;
            ResponsePacket? ret = null;

            if (extFolderMap.TryGetValue(ext, out extInfo))
            {
                // Strip off leading '/' and reformat as with windows path separator.
                if (path.StartsWith("/") || path.StartsWith("\\"))
                    path = path.Substring(1);

                string fullPath = Path.Combine(WebsitePath, path);
                ret = extInfo.Loader(fullPath, ext, extInfo);
            }

            return ret;
        }
    }
}
