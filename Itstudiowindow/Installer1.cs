using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Itstudiowindow
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
            this.AfterInstall += new InstallEventHandler(AfterInstallEventHandler);
            this.AfterUninstall += new InstallEventHandler(AfterUninstallEventHandler);
        }
        public void AfterInstallEventHandler(object sender, InstallEventArgs e)
        {
            string path = this.Context.Parameters["targetdir"];
            string path_rdpwrap = this.Context.Parameters["targetdir"];
            // 将/替换为\
            path = path.Replace("/", @"\");
            string replacedString = path.Replace(@"\\", @"\");
            if (string.IsNullOrEmpty(path))
            {
                Logger("path为空");
            }
            else
            {
                path = $@"{replacedString}Itstudiowindow.exe";
                path_rdpwrap = $@"{replacedString}rdpwrap.dll";
                Logger($"path: {path}");
            }
            // 关闭UAC默认提示，设置为0
            try
            {
                // 定义要修改的注册表路径和值名
                string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                string valueName = "ConsentPromptBehaviorAdmin";

                // 打开注册表键
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true))
                {
                    if (key != null)
                    {
                        // 设置注册表值为0
                        key.SetValue(valueName, 0, RegistryValueKind.DWord);
                        Logger($"注册表项 '{valueName}' 已成功设置为 0.");
                        Console.WriteLine($"注册表项 '{valueName}' 已成功设置为 0.");
                    }
                    else
                    {
                        Logger($"无法打开注册表键 '{subKey}'.");
                        Console.WriteLine($"无法打开注册表键 '{subKey}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger($"发生错误: {ex.Message}");
                Console.WriteLine($"发生错误: {ex.Message}");
            }


            // 设置会话断开存活时间

            try
            {

                string keyPath = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services";
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
                                newKey.SetValue("MaxDisconnectionTime", 1, RegistryValueKind.DWord);
                                newKey.SetValue("fDisableAutoReconnect", 0, RegistryValueKind.DWord);
                                newKey.SetValue("fEnableRemoteFXAdvancedRemoteApp", 1, RegistryValueKind.DWord);
                                newKey.SetValue("UseUniversalPrinterDriverFirst", 3, RegistryValueKind.DWord);
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
                            key.SetValue("MaxDisconnectionTime", 1, RegistryValueKind.DWord);
                            key.SetValue("fDisableAutoReconnect", 0, RegistryValueKind.DWord);
                            key.SetValue("fEnableRemoteFXAdvancedRemoteApp", 1, RegistryValueKind.DWord);
                            key.SetValue("UseUniversalPrinterDriverFirst", 3, RegistryValueKind.DWord);
                            Console.WriteLine("已更新注册表值。");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("无法更新注册表值：" + ex.Message);
                        }
                    }
                }



                // 查看SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList下面是否有Applications，如果没有就添加
                keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList";
                string subKeyName = "Applications";

                // 检查是否存在Applications子项
                using (RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (parentKey == null || parentKey.OpenSubKey(subKeyName) == null)
                    {
                        // 如果不存在，则添加Applications子项
                        using (RegistryKey tsAppAllowListKey = Registry.LocalMachine.OpenSubKey(keyPath, true))
                        {
                            if (tsAppAllowListKey == null)
                            {
                                Console.WriteLine("指定的注册表键不存在。");
                                return;
                            }

                            tsAppAllowListKey.CreateSubKey(subKeyName);
                            Console.WriteLine("Applications子项已成功添加。");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Applications子项已经存在。");
                    }
                }




                // 定义注册表路径
                keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList\Applications";

                // 检查是否存在 "Itstudio" 项
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null && key.GetValue("Itstudio") != null)
                    {
                        // 如果存在 "Itstudio" 项，则更新其值
                        using (RegistryKey subkey = key.OpenSubKey("Itstudio", true))
                        {
                            subkey.SetValue("CommandLineSetting", 1, RegistryValueKind.DWord);
                            subkey.SetValue("IconIndex", 0, RegistryValueKind.DWord);
                            subkey.SetValue("IconPath", $@"{path}", RegistryValueKind.String);
                            subkey.SetValue("Name", "Itstudio", RegistryValueKind.String);
                            subkey.SetValue("Path", $@"{path}", RegistryValueKind.String);
                            subkey.SetValue("RequiredCommandLine", "", RegistryValueKind.String);
                            subkey.SetValue("ShowInTSWA", 0, RegistryValueKind.DWord);
                            subkey.SetValue("VPath", $@"{path}", RegistryValueKind.String);
                        }
                    }
                    else
                    {
                        // 如果不存在 "Itstudio" 项，则创建它并添加值
                        using (RegistryKey subkey = key.CreateSubKey("Itstudio"))
                        {
                            subkey.SetValue("CommandLineSetting", 1, RegistryValueKind.DWord);
                            subkey.SetValue("IconIndex", 0, RegistryValueKind.DWord);
                            subkey.SetValue("IconPath", $@"{path}", RegistryValueKind.String);
                            subkey.SetValue("Name", "Itstudio", RegistryValueKind.String);
                            subkey.SetValue("Path", $@"{path}", RegistryValueKind.String);
                            subkey.SetValue("RequiredCommandLine", "", RegistryValueKind.String);
                            subkey.SetValue("ShowInTSWA", 0, RegistryValueKind.DWord);
                            subkey.SetValue("VPath", $@"{path}", RegistryValueKind.String);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }

            //string version = GetOperatingSystemVersion();
            LoadFile(path_rdpwrap);

            try
            {
                // 停止服务
                Process stopProcess = new Process();
                stopProcess.StartInfo.FileName = "powershell.exe";
                stopProcess.StartInfo.Arguments = "Stop-Service -Name TermService -Force";
                stopProcess.StartInfo.CreateNoWindow = true; // 隐藏窗口
                stopProcess.StartInfo.RedirectStandardOutput = true; // 重定向输出
                stopProcess.StartInfo.UseShellExecute = false; // 不使用系统外壳程序，允许重定向
                stopProcess.Start();
                stopProcess.WaitForExit();

                // 检查停止服务是否成功
                if (stopProcess.ExitCode == 0)
                {
                    // 启动服务
                    Process startProcess = new Process();
                    startProcess.StartInfo.FileName = "powershell.exe";
                    startProcess.StartInfo.Arguments = "Start-Service -Name TermService";
                    startProcess.StartInfo.CreateNoWindow = true;
                    startProcess.StartInfo.RedirectStandardOutput = true;
                    startProcess.StartInfo.UseShellExecute = false;
                    startProcess.Start();
                    startProcess.WaitForExit();

                    // 可以根据startProcess.ExitCode检查启动服务是否成功

                    if (startProcess.ExitCode == 0)
                    {
                        Logger("服务已成功启动。");
                    }
                    else
                    {
                        Logger("启动TermService服务失败。");
                    }
                }
                else
                {
                    Logger("停止TermService服务失败。");
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }

        }

        static string GetOperatingSystemVersion()
        {
            var os = Environment.OSVersion;
            var version = os.Version;

            if (os.Platform == PlatformID.Win32NT)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                    {
                        if (key != null)
                        {
                            int currentBuild = int.Parse(key.GetValue("CurrentBuild").ToString());

                            if (version.Major >= 10)
                            {
                                if (currentBuild >= 22000)
                                {
                                    
                                    return "Windows 11";
                                }
                                else
                                {
                                    
                                    return "Windows 10";
                                }
                            }
                            else if (version.Major == 6 && version.Minor == 2)
                            {
                                // Windows 8, Windows Server 2012
                                return "Windows 8 / Windows Server 2012";
                            }
                            else if (version.Major == 6 && version.Minor == 1)
                            {
                                // Windows 7, Windows Server 2008 R2
                                return "Windows 7 / Windows Server 2008 R2";
                            }
                            else if (version.Major == 6 && version.Minor == 0)
                            {
                                // Windows Vista, Windows Server 2008
                                return "Windows Vista / Windows Server 2008";
                            }
                            else if (version.Major == 5 && version.Minor >= 1)
                            {
                                // Windows XP, Windows Server 2003
                                return "Windows XP / Windows Server 2003 or earlier";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing registry: {ex.Message}");
                    Logger($"Error accessing registry: {ex.Message}");
                    return $"{ex.Message}";
                }
            }
            else
            {
                return "Non-Windows Operating System";
            }

            return "Unknown Windows Version";
        }
        // 修改注册表远程 DLL 文件路径
        static void LoadFile(string path)
        {
            try
            {
                // 将注册表：计算机\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TermService\Parameters下的ServiceDll设置为%ProgramFiles%\RDP Wrapper\rdpwrap.dll
                string keyPath = @"SYSTEM\CurrentControlSet\Services\TermService\Parameters";
                string valueName = "ServiceDll";
                string newValue = path;

                // 检查注册表键是否存在
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine("指定的注册表键不存在。");
                        Logger("指定的注册表键不存在。");
                        return;
                    }

                    // 设置值
                    key.SetValue(valueName, newValue, RegistryValueKind.ExpandString);
                    Logger("已更新注册表值。");
                    Console.WriteLine("已更新注册表值。");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger("没有足够的权限访问目标文件夹。请确保你有管理员权限。");
                Console.WriteLine("没有足够的权限访问目标文件夹。请确保你有管理员权限。");
            }
            catch (FileNotFoundException)
            {
                Logger("源文件未找到，请确保rdpwrap.ini和rdpwrap.dll位于程序当前目录。");
                Console.WriteLine("源文件未找到，请确保rdpwrap.ini和rdpwrap.dll位于程序当前目录。");
            }
            catch (Exception ex)
            {
                Logger("发生错误：" + ex.Message);
                Console.WriteLine("发生错误：" + ex.Message);
            }
        
        }


        static void AfterUninstallEventHandler(object sender, InstallEventArgs e)
        {
            // 恢复注册表项

            try
            {
                // 恢复注册表项
                string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
                string valueName = "ConsentPromptBehaviorAdmin";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, 2, RegistryValueKind.DWord);
                        Logger($"注册表项 '{valueName}' 已成功设置为 2.");

                    }
                    else
                    {
                        Logger($"无法打开注册表键 '{subKey}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger($"发生错误: {ex.Message}");
            }

            // 恢复注册表项
            try
            {
                string keyPath = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("MaxDisconnectionTime");
                        key.DeleteValue("fDisableAutoReconnect");
                        key.DeleteValue("fEnableRemoteFXAdvancedRemoteApp");
                        key.DeleteValue("UseUniversalPrinterDriverFirst");
                        Console.WriteLine("已删除注册表值。");
                    }
                    else
                    {
                        Console.WriteLine("指定的注册表键不存在。");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }

            

            // 恢复注册表项
            try
            {
                string keyPath = @"SYSTEM\CurrentControlSet\Services\TermService\Parameters";
                string valueName = "ServiceDll";
                string newValue = @"%SystemRoot%\System32\termsrv.dll";

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, newValue, RegistryValueKind.ExpandString);
                        Console.WriteLine("已更新注册表值。");
                    }
                    else
                    {
                        Console.WriteLine("指定的注册表键不存在。");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }

            try
            {
                // 停止服务
                Process stopProcess = new Process();
                stopProcess.StartInfo.FileName = "powershell.exe";
                stopProcess.StartInfo.Arguments = "Stop-Service -Name TermService -Force";
                stopProcess.StartInfo.CreateNoWindow = true; // 隐藏窗口
                stopProcess.StartInfo.RedirectStandardOutput = true; // 重定向输出
                stopProcess.StartInfo.UseShellExecute = false; // 不使用系统外壳程序，允许重定向
                stopProcess.Start();
                stopProcess.WaitForExit();

                // 检查停止服务是否成功
                if (stopProcess.ExitCode == 0)
                {
                    // 启动服务
                    Process startProcess = new Process();
                    startProcess.StartInfo.FileName = "powershell.exe";
                    startProcess.StartInfo.Arguments = "Start-Service -Name TermService";
                    startProcess.StartInfo.CreateNoWindow = true;
                    startProcess.StartInfo.RedirectStandardOutput = true;
                    startProcess.StartInfo.UseShellExecute = false;
                    startProcess.Start();
                    startProcess.WaitForExit();

                    // 可以根据startProcess.ExitCode检查启动服务是否成功

                    if (startProcess.ExitCode == 0)
                    {
                        Logger("服务已成功启动。");
                    }
                    else
                    {
                        Logger("启动TermService服务失败。");
                    }
                }
                else
                {
                    Logger("停止TermService服务失败。");
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }

        }


        private static void Logger(string message)
        {
            try
            {
                string fileName = @"C:\log.txt";
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
