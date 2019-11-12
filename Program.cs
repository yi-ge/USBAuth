using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace USBAuth
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "s")
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new USBAuth()
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                Console.WriteLine("通过 DigiSpark 实现 Windows 免密登录");
                Console.WriteLine("");
                Console.WriteLine("注意：添加设备前请先勿插入设备。");
                Console.WriteLine("");
                Console.WriteLine("请选择：[1]添加设备 [2]安装并启动服务 [3]卸载服务 [4]退出");
                var rs = int.Parse(Console.ReadLine());
                switch (rs)
                {
                    case 1:
                        USBAuth serviceMain = new USBAuth();
                        serviceMain.AddDevice();
                        Console.WriteLine("");
                        Console.WriteLine("");
                        Console.WriteLine("");
                        Main(args);
                        break;
                    case 2:
                        //取当前可执行文件路径，加上"s"参数，证明是从windows服务启动该程序
                        var path = Process.GetCurrentProcess().MainModule.FileName + " s";
                        Process.Start("sc", "create USBAuth binpath= \"" + path + "\" displayName=USBAuth start=auto");
                        Process.Start("sc", "start USBAuth");
                        Console.WriteLine("安装并启动成功。按任意键退出。");
                        Console.Read();
                        break;
                    case 3:
                        Process.Start("sc", "delete USBAuth");
                        Console.WriteLine("卸载成功。按任意键退出。");
                        Console.Read();
                        break;
                    case 4: break;
                }
            }
        }
    }
}
