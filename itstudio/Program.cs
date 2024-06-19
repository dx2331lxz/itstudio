// 导入HttpListener
using System.Net;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

// using IWshRuntimeLibrary;


namespace itstudio
{
    internal class Program
    {
        static HttpListener httpobj;
        public static void Main(string[] args)
        {

            string keyPath = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services";
            string valueName = "RemoteAppLogoffTimeLimit";
            int newValue = 6000; // 新的值

            // 检查注册表键是否存在
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
            {
                if (key == null)
                {
                    // 如果键不存在，则创建
                    using (RegistryKey newKey = Registry.LocalMachine.CreateSubKey(keyPath))
                    {
                        if (newKey != null)
                        {
                            // 设置值
                            newKey.SetValue(valueName, newValue, RegistryValueKind.DWord);
                            Console.WriteLine("已添加注册表键和值。");
                        }
                        else
                        {
                            Console.WriteLine("无法创建注册表键。");
                        }
                    }
                }
                else
                {
                    // 如果键存在，则更新值
                    try
                    {
                        key.SetValue(valueName, newValue, RegistryValueKind.DWord);
                        Console.WriteLine("已更新注册表值。");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("无法更新注册表值：" + ex.Message);
                    }
                }
            }
            valueName = "MaxDisconnectionTime";
            newValue = 60000;
            // 检查注册表键是否存在
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
            {
                if (key == null)
                {
                    // 如果键不存在，则创建
                    using (RegistryKey newKey = Registry.LocalMachine.CreateSubKey(keyPath))
                    {
                        if (newKey != null)
                        {
                            // 设置值
                            newKey.SetValue(valueName, newValue, RegistryValueKind.DWord);
                            Console.WriteLine("已添加注册表键和值。");
                        }
                        else
                        {
                            Console.WriteLine("无法创建注册表键。");
                        }
                    }
                }
                else
                {
                    // 如果键存在，则更新值
                    try
                    {
                        key.SetValue(valueName, newValue, RegistryValueKind.DWord);
                        Console.WriteLine("已更新注册表值。");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("无法更新注册表值：" + ex.Message);
                    }
                }
            }


            Console.ReadKey();
                
        }
        private static void LoadFile()
            {
                string targetFolderPath = @"C:\Program Files\RDP Wrapper"; // 目标文件夹路径
                string iniFilePath = "rdpwrap.ini"; // rdpwrap.ini文件路径在当前程序目录下的Win10文件夹中
                string dllFilePath = "rdpwrap.dll";

                try
                {
                    // 使用当前程序的基目录来构造完整的源文件路径
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    iniFilePath = Path.Combine(currentDirectory, iniFilePath);
                    dllFilePath = Path.Combine(currentDirectory, dllFilePath);

                    // 检查目标文件夹是否存在，如果不存在则创建
                    if (!Directory.Exists(targetFolderPath))
                    {
                        Directory.CreateDirectory(targetFolderPath);
                        Console.WriteLine("目标文件夹已创建: " + targetFolderPath);
                    }
                    else
                    {
                        Console.WriteLine("目标文件夹已存在: " + targetFolderPath);
                    }

                    // 复制rdpwrap.ini到目标文件夹
                    File.Copy(iniFilePath, Path.Combine(targetFolderPath, Path.GetFileName(iniFilePath)), true);
                    Console.WriteLine("rdpwrap.ini已复制到目标文件夹");

                    // 复制rdpwrap.dll到目标文件夹
                    File.Copy(dllFilePath, Path.Combine(targetFolderPath, Path.GetFileName(dllFilePath)), true);
                    Console.WriteLine("rdpwrap.dll已复制到目标文件夹");

                    Console.WriteLine("操作完成！");


                    // 将注册表：计算机\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TermService\Parameters下的ServiceDll设置为%ProgramFiles%\RDP Wrapper\rdpwrap.dll
                    string keyPath = @"SYSTEM\CurrentControlSet\Services\TermService\Parameters";
                    string valueName = "ServiceDll";
                    string newValue = @"%ProgramFiles%\RDP Wrapper\rdpwrap.dll";

                    // 检查注册表键是否存在
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                    {
                        if (key == null)
                        {
                            Console.WriteLine("指定的注册表键不存在。");
                            return;
                        }

                        // 设置值
                        key.SetValue(valueName, newValue, RegistryValueKind.ExpandString);
                        Console.WriteLine("已更新注册表值。");
                    }
                    // 重启远程桌面服务
                    Console.WriteLine("正在重启远程桌面服务...");
                    System.Diagnostics.Process.Start("net", "stop TermService");
                    System.Diagnostics.Process.Start("net", "start TermService");
                    Console.WriteLine("远程桌面服务已重启。");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("没有足够的权限访问目标文件夹。请确保你有管理员权限。");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("源文件未找到，请确保rdpwrap.ini和rdpwrap.dll位于程序当前目录。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发生错误：" + ex.Message);
                }

            }

            static bool IsPortOpen(int port)
            {
                string command = $"netsh advfirewall firewall show rule name=\"Open Port {port}\"";
                string output = ExecuteCommand(command);
                // 检查输出中是否包含规则信息
                return output.Contains($"Port={port}");
            }
            static void OpenPort(int port)
            {
                string command = $"netsh advfirewall firewall add rule name=\"Open Port {port}\" dir=in action=allow protocol=TCP localport={port}";
                ExecuteCommand(command);
                Console.WriteLine($"Port {port} is now open in Windows Firewall.");
            }
            // 执行cmd命令
            static string ExecuteCommand(string command)
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c " + command;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true; // 在不显示命令行窗口的情况下执行命令
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
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
                        string path = HttpUtility.UrlDecode(query["path"]);
                        if (path == null)
                        {
                            responseString =
                                "文件路径为空，请在url中添加path参数，如http://localhost:14915/?path=C:/Users/itstudio/Desktop/1.txt";
                        }
                        else
                        {
                            Logger($"open {path}");
                            // base64解码
                            path = Encoding.UTF8.GetString(Convert.FromBase64String(path));
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
                        var json = File.ReadAllText("C:\\Users\\28729\\RiderProjects\\itstudio\\itstudio\\lists.json");
                        responseString = json;
                    }
                    else if (request.Url.AbsolutePath == "/start/")
                    {
                        // 打开当前目录下的remoteApp_win.exe
                        System.Diagnostics.Process.Start("remoteApp_win.exe");
                    }
                    else
                    {
                        responseString = "404 Not Found";
                    }


                    var returnByteArr = Encoding.UTF8.GetBytes(responseString);
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

            // 根据.lnk文件路径打开对应的文件
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
                    return false;
                }
            }

            private static void Logger(string message)
            {
                try
                {
                    string fileName = @"log.txt";
                    Console.WriteLine(fileName);
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        // 写入内容到文件
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + message);
                    }
                }
                catch (Exception ex)
                {
                    string fileName = @"log.txt";
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        // 写入内容到文件
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + ex.Message);
                    }
                }
            }
        }
    }
