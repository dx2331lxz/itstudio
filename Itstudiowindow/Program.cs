using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Web;

namespace Itstudiowindow
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static HttpListener httpobj;
        [STAThread]
        static void Main()
        {
            //Logger("Port 14915 is already open in Windows Firewall.");
            // 确保只有一个Itstudiowindow.exe在运行
            Process[] processes = Process.GetProcessesByName("Itstudiowindow");
            if (processes.Length > 1)
            {
                // 关闭除当前进程以外的其他进程
                foreach (var process in processes)
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        try
                        {
                            // 关闭进程
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }

            httpobj = new HttpListener();
            httpobj.Prefixes.Add("http://127.0.0.1:14915/");
            httpobj.Start();
            httpobj.BeginGetContext(Result, null);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new 远程应用管理());
            


        }
        public static void Result(IAsyncResult ar)
        {
            httpobj.BeginGetContext(Result, null);
            var guid = Guid.NewGuid().ToString();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{guid}]请求已接收");
            var context = httpobj.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            var responseString = "";
            if (request.HttpMethod == "POST")
            {

                responseString = HandleRequest(request, response);
            }
            else if (request.HttpMethod == "GET")
            {
                Console.WriteLine(request.Url.AbsolutePath);
                if (request.Url.AbsolutePath == "/")
                {
                    // 获取get请求的参数
                    var query = request.QueryString;
                    // 打印参数名为path的值
                    string path = query["path"];
                    if (path == null)
                    {
                        responseString =
                            "文件路径为空，请在url中添加path参数，如http://127.0.0.1:14915/?path=C:/Users/itstudio/Desktop/1.txt";
                    }
                    else
                    {
                        Logger($"open {path}");
                        // 把path中的空格换成+
                        path = path.Replace( ' ', '+' );
                        // base64解码
                        try
                        {
                            path = Encoding.UTF8.GetString(Convert.FromBase64String(path));
                            Logger($"open {path}");
                        }
                        catch (Exception e)
                        {
                            Logger(e.Message);
                        }

                        var result = OpenFile(path);
                        if (result)
                        {
                            responseString = "文件打开成功";
                        }
                        else
                        {
                            responseString = "文件打开失败";
                        }

                    }
                }
                else if (request.Url.AbsolutePath == "/list/")
                {
                    // 读取json文件
                    //var json = File.ReadAllText("list.json");
                    try
                    {
                        var json = File.ReadAllText("list.json");
                        responseString = json;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        responseString = "{}";
                    }
                }
                else if (request.Url.AbsolutePath == "/start/")
                {
                    // 打开当前目录下的remoteApp_win.exe
                    Process[] processes = Process.GetProcessesByName("remoteApp_win");
                    if (processes.Length > 0)
                    {
                        foreach (var process in processes)
                        {
                            try
                            {
                                // 关闭进程
                                process.Kill();

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    else
                    {

                    }
                    System.Diagnostics.Process.Start("remoteApp_win.exe");
                    responseString = "启动成功";
                }
                else
                {
                    responseString = "404 Not Found";
                }

                // Base64编码
                responseString = Convert.ToBase64String(Encoding.UTF8.GetBytes(responseString));


                var returnByteArr = Encoding.Default.GetBytes(responseString);
                
                try
                {
                    using (var stream = response.OutputStream)
                    {
                        stream.Write(returnByteArr, 0, returnByteArr.Length);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("请求处理失败");
                }


            }

        }
        public static bool OpenFile(string path)
        {
            // 打开文件
            try
            {
                System.Diagnostics.Process.Start(path);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Logger(e.Message);
                return false;
            }
        }
        private static string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string data = null;
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                do
                {
                    readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray());

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Console.WriteLine(data);
            return "ok";
        }
        private static void Logger(string message)
        {
            try
            {
                string fileName = @"log.txt";
                Console.WriteLine(fileName);
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // 写入内容到文件
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + message);
                }
            }
            catch (Exception ex)
            {
                string fileName = @"log.txt";
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // 写入内容到文件
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + ex.Message);
                }
            }
        }
    }
}
