using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
// 引入File
using System.IO;

namespace installing
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {

            string path = this.Context.Parameters["targetdir"];
            path = $@"{path}\itstudio.exe";
            Logger($"path: {path}");

            // 根据路径添加注册表

            path = $@"{path}\itstudio.exe";


            if (string.IsNullOrEmpty(path))
            {
                path = "itstudio";
            }

            try
            {
                // 定义注册表路径
                string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList\Applications";

                // 检查是否存在 "Itstudio" 项
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        if (key.GetValue("Itstudio") != null)
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
                    else
                    {
                        Logger("注册表路径不存在");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger("错误：" + ex.Message);
            }
        }


        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        private static void Logger(string message)
        {
            try
            {
                string fileName = @"L:\临时文件\log.txt";
                Console.WriteLine(fileName);
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // 写入内容到文件
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + message);
                }
            }
            catch (Exception ex)
            {
                string fileName = @"L:\临时文件\log.txt";
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    // 写入内容到文件
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" + ex.Message);
                }
            }
        }
    }
}
