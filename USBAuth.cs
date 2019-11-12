using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USBAuth
{
    public partial class USBAuth : ServiceBase
    {
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSEnumerateSessions(IntPtr hServer, int Reserved, int Version, ref IntPtr ppSessionInfo, ref int pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public Int32 SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        private enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public static void LockWorkStation()
        {
            IntPtr ppSessionInfo = IntPtr.Zero;
            Int32 count = 0;
            Int64 retval = WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref ppSessionInfo, ref count);
            Int64 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int64 currentSession = (long)ppSessionInfo;

            if (retval == 0) return;

            for (int i = 0; i < count; i++)
            {
                WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)currentSession, typeof(WTS_SESSION_INFO));
                if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive) WTSDisconnectSession(IntPtr.Zero, si.SessionID, false);
                currentSession += dataSize;
            }
            WTSFreeMemory(ppSessionInfo);
        }

        public USBAuth()
        {
            InitializeComponent();
        }

        private bool hasDevice(string deviceId)
        {
            var scope = new ManagementScope();
            var query = new SelectQuery("select * from Win32_Keyboard");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                var collection = searcher.Get();
                foreach (var obj in collection)
                {
                    if (obj.GetPropertyValue("DeviceID").ToString() == deviceId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private List<string> getDeviceIDList()
        {
            var scope = new ManagementScope();
            var query = new SelectQuery("select * from Win32_Keyboard");
            List<string> deviceIDList = new List<string>();

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                var collection = searcher.Get();
                foreach (var obj in collection)
                {
                    deviceIDList.Add(obj.GetPropertyValue("DeviceID").ToString());
                }
            }

            return deviceIDList;
        }

        public void Run()
        {

            string deviceId = File.ReadAllText(@"C:\Windows\USBAuth\DeviceID", Encoding.UTF8);
            Task.Run(() =>
            {
                while (true)
                {
                    if (!hasDevice(deviceId))
                    {
                        LockWorkStation();
                    }
                    Thread.Sleep(5000);
                }
            });
        }

        public void AddDevice()
        {
            List<string> deviceIDList = getDeviceIDList();

            Console.WriteLine("请插入您的 DigiSpark 设备...");

            while (true)
            {
                List<string> deviceIDListNew = getDeviceIDList();
                if (deviceIDList.Count != deviceIDListNew.Count)
                {
                    var exceptVal = deviceIDListNew.Except(deviceIDList).ToList();
                    if (exceptVal.Count != 1)
                    {
                        Console.WriteLine("请检查是否同时插入/移除了多个设备");
                    }
                    else
                    {
                        if (!Directory.Exists(@"C:\Windows\USBAuth")) Directory.CreateDirectory(@"C:\Windows\USBAuth");
                        File.WriteAllText(@"C:\Windows\USBAuth\DeviceID", exceptVal[0], Encoding.UTF8);
                        Console.WriteLine("设备已添加。");
                        return;
                    }
                }
                Thread.Sleep(3000);
            }
        }

        protected override void OnStart(string[] args)
        {
            Run();
        }

        protected override void OnStop()
        {
        }
    }
}
