using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WebServer
{
    /// <summary>
    /// A lean and mean web server.
    /// </summary>
    public static class Server
    {
        private static HttpListener listener;
        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
        private static Router router = new();

        /// <summary>
        /// Returns list of IP addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
        /// </summary>
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToList();

            return ret;
        }

        private static HttpListener InitializeListener(List<IPAddress> localkhostIPs)
        {
            HttpListener listener = new();
            listener.Prefixes.Add("http://localhost/");

            // Listen to IP address
            localkhostIPs.ForEach(ip =>
            {
                Console.WriteLine("Listening on IP " + "http://" + ip.ToString() + "/");
                listener.Prefixes.Add("http://" + ip.ToString() + "/");
            });

            return listener;
        }

        // <summary>
        /// Begin listening to connections on a separate worker thread.
        /// </summary>
        /// 
        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        /// <summary>
        /// Start awaiting for connections, up to the "maxSimultaneousConnections" value.
        /// This code runs in a separate thread.
        /// </summary>
        private static void RunServer(HttpListener listener) 
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        /// <summary>
        /// Await connections.
        /// </summary>
        private static async void StartConnectionListener(HttpListener listener)
        {
            ResponsePacket responsePacket = null;

            // Wait for a connection. Return to caller while we wait.
            HttpListenerContext context = await listener.GetContextAsync();

            // Release the semaphore so that another listener can be immediately started up.
            sem.Release();

            Log(context.Request);

            HttpListenerRequest request = context.Request;
            string rawUrl = request.RawUrl!;
            int index = rawUrl.IndexOf("?");

            string verb = request.HttpMethod; // get, post, delete, etc.
            string path = (index >= 0) ? rawUrl.Substring(0, index) : rawUrl; // Only the path, not any of the parameters.
            string parameters = (index >= 0) ? rawUrl.Substring(index + 1) : string.Empty; // Params on the URL itself follow the URL and are separated by a ?.

            Dictionary<string, object> kvParams = GetKeyValues(parameters);

            // We have a connection, do something...
            responsePacket = router.Route(verb, path, kvParams);

            if (responsePacket == null)
            {
                var resp = context.Response;
                resp.StatusCode = (int)HttpStatusCode.NotFound;
                byte[] notFound = Encoding.UTF8.GetBytes("404 – Resource not found");
                resp.ContentLength64 = notFound.Length;
                resp.OutputStream.Write(notFound, 0, notFound.Length);
                resp.OutputStream.Close();
                return;
            }

            Respond(context.Response, responsePacket);
        }

        /// <summary>
        /// Sends an HTTP response by setting the headers and encoding from the response packet,
        /// writing its data to the output stream, and closing the stream with an OK status.
        /// </summary>
        private static void Respond(HttpListenerResponse response, ResponsePacket respPacket)
        {
            response.ContentType = respPacket.ContentType;
            response.ContentLength64 = respPacket.Data.Length;
            response.OutputStream.Write(respPacket.Data, 0, respPacket.Data.Length);
            response.ContentEncoding = respPacket.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        public static void Start(string websitePath)
        {
            router.WebsitePath = websitePath;
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }

        /// <summary>
        /// Log requests.
        /// </summary>
        public static void Log(HttpListenerRequest request)
        {
            string uri = request.Url!.AbsoluteUri;
            int index = -1;
            for (int i = 0; i < 3; i++)
            {
                index = uri.IndexOf('/', index + 1);
                if (index == -1)
                    break;
            }
            string result = (index != -1 && index < uri.Length - 1) ? uri.Substring(index + 1) : string.Empty;

            Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + result);
        }

        /// <summary>
        /// Separate out key-value pairs, delimited by & and into individual key-value instances, separated by =
		/// Ex input: username=abc&password=123
        /// </summary>
        private static Dictionary<string, object> GetKeyValues(string data, Dictionary<string, object> kv = null)
        { 
            if (kv is null)
                kv = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(data))
            {
                string[] pairs = data.Split('&');
                foreach (var pair in pairs)
                {
                    int pos = pair.IndexOf('=');
                    if (pos > -1)
                    {
                        string key = pair.Substring(0, pos);
                        string value = pair.Substring(pos + 1);
                        value = Uri.UnescapeDataString(value);
                        kv[key] = value;
                    }
                    else
                        kv[pair] = null;                    
                }
            }

            return kv;
        }
    }
}