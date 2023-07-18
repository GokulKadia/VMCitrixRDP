using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static VMWareView.VMWare;
using System.IO;

namespace VMWareView
{
    //regPath=HKEY_CURRENT_USER\SOFTWARE\VMware, Inc.\VMware VDM\Client  
    public class VMWare
    {
        static string VM_VIEW_INSTALL_PATH = "Software\\VMware, Inc.\\VMware VDM";
        static string VM_VIEW_INSTALL_PATHWOW = "SOFTWARE\\WOW6432Node\\VMware, Inc.\\VMware VDM";
        static string VM_VIEW_REG_PATH = "Software\\VMware, Inc.\\VMware VDM\\Client"; //x86
        static string VM_VIEW_REG_PATH_WOW = "SOFTWARE\\Wow6432Node\\VMware, Inc.\\VMware VDM\\Client"; //x64
        static string VM_VIEW_SECURITY_REG_PATH = "Software\\VMware, Inc.\\VMware VDM\\Client\\Security";
        static string VM_VIEW_SECURITY_REG_PATH_WOW = "SOFTWARE\\Wow6432Node\\VMware, Inc.\\VMware VDM\\Client\\Security";
        static string VM_VIEW_INSTALL_PATH_WOW = "Software\\Wow6432Node\\VMware, Inc.\\VMware VDM";

        static string VMVIEW_DEFAULT_CONNECTION_NAME_LNK = "_VMware Horizon Client.lnk";
        /// <summary>
        /// This whole function will fetch the OS Architecture and return the installed path based upon OS
        /// </summary>
        internal const ushort PROCESSOR_ARCHITECTURE_x86 = 0;
        internal const ushort PROCESSOR_ARCHITECTURE_x64 = 9;
        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
        };
        [DllImport("Kernel32")]
        static extern void GetNativeSystemInfo(out SYSTEM_INFO systemInfo);
        [DllImport("shell32.dll")]
        static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags, [Out] StringBuilder pszPath);

        /// <summary>
        /// this function is for Fetching the installed path of the VMware
        /// </summary>
        /// <param name="path"></param>
        public static void GetVMinstalledpath(out string path)
        {
            path = "";
            if (getOSArchitecture() == "x64")
            {
                var regTimeZones = Registry.LocalMachine.OpenSubKey(VM_VIEW_INSTALL_PATHWOW);
                path = regTimeZones.GetValue("ClientInstallPath").ToString();
            }
            else
            {
                //We have to write logic for x86 vmware ClientInstallPath
            }
        }

        /// <summary>
        /// This is for Fetching the SYstem architecture
        /// </summary>
        /// <returns></returns>
        static string getOSArchitecture()
        {
            SYSTEM_INFO systemInfo1;
            string OSArchitecture;
            GetNativeSystemInfo(out systemInfo1);
            OSArchitecture = systemInfo1.wProcessorArchitecture == 9 ? "x64" : "x86";
            return OSArchitecture;
        }

        /// <summary>
        /// this will create the config file in document/vmware 
        /// </summary>
        public static void CreateConfigFile()
        {
            string wsCommandLine = "";
            string desktopProtocol = "PCoIP";
            string loginasCurrentUser = "false";
            GetVMinstalledpath(out string path);
            if (Directory.Exists(path)) wsCommandLine += "-desktopProtocol ";
            wsCommandLine += desktopProtocol == "PCoIP" ? "PCoIP" : desktopProtocol == "RDP" ? "RDP" : desktopProtocol == "Blast" ? "Blast" : "PCoIP";
            wsCommandLine += "\n";
            wsCommandLine += "-singleAutoConnect";
            wsCommandLine += "\n";
            wsCommandLine += "-loginAsCurrentUser";
            wsCommandLine += " ";
            wsCommandLine += loginasCurrentUser == "true" ? "true" : "false";
            wsCommandLine += "\n";
            wsCommandLine += "-appSessionReconnectionBehavior";
            wsCommandLine += " ";
            wsCommandLine += "never";

            string DocPath = GetSpecialFolder(0x0005);
            DocPath += "\\VMware\\";
            _ = !Directory.Exists(DocPath) ? Directory.CreateDirectory(DocPath) : null;
            DocPath += "Config.txt";
            File.WriteAllText(DocPath, wsCommandLine);

        }

        /// <summary>
        /// this will return the path for Document and Desktop based on PathSpecifier 
        /// </summary>
        public static string GetSpecialFolder(int PathSpecifier)
        {
            const int MaxPath = 260;
            StringBuilder builder = new StringBuilder(MaxPath);
            SHGetFolderPath(IntPtr.Zero, PathSpecifier, IntPtr.Zero, 0x0000, builder);
            return builder.ToString();
        }

        /// <summary>
        /// this will update the Registry for which ever argument or setting we configured in VM
        /// </summary>
        /// <returns></returns>
        public static bool UpdateVmViewRegistry()
        {
            //In this path VM_VIEW_REG_PATH and VM_VIEW_REG_PATH_WOW we have to set registry for Vware for 
            var regTimeZones = Registry.LocalMachine.OpenSubKey(VM_VIEW_INSTALL_PATH);
            
            return true;
        }

        /// <summary>
        /// This willl create the Shortcut
        /// </summary>
        public static void CreateShortcut()
        {
            string wsCommandLine = "";
            string DocPath = GetSpecialFolder(0x0005);
            DocPath += "\\VMware\\";
            _ = !Directory.Exists(DocPath) ? Directory.CreateDirectory(DocPath) : null;
            DocPath += "Config.txt";
            wsCommandLine += " ";
            wsCommandLine += "-file";
            wsCommandLine += " ";
            wsCommandLine += DocPath;

            string connectionLink = "";
            string VMpath = "";
            if (getOSArchitecture()=="x64")
            {
                GetVMinstalledpath( out VMpath);
                VMpath += "vmware-view.exe";
                connectionLink = "TestVM";
                connectionLink += VMVIEW_DEFAULT_CONNECTION_NAME_LNK;
                string path = GetSpecialFolder((int)Path_Specifier.DESKTOPDIRECTORY);
                path += "\\";
                path += connectionLink;
                Shortcut.Create(path, VMpath, wsCommandLine, null,null,null);
            }
        }


        /*
         * First we need to check the getVMViewInstallPath  based on system architecture x64 n x86
         * CreateConfigFile
         * Update the Registry for VMware with IP and all details
         * Create ShortCut on Desktop
         * is Certmode is there then we need to Add registry in VMware wow path depends on x64 n x86
         * AutoStart function goes after this that will run auto start
         * 
        */

    }

    public enum Path_Specifier
    {
        System_Path = 0x0025,
        PROGRAM_FILES = 0x0026,
        MYPICTURES= 0x0027,
        MyDocument= 0x0005,
        STARTUP = 0x0007,
        COMMON_DESKTOPDIRECTORY= 0x0019,
        DESKTOPDIRECTORY = 0x0010,
        PROGRAM_FILESX86= 0x002a,
        PROGRAM_FILES_COMMONX86= 0x002c,
        CSIDL_COMMON_DOCUMENTS= 0x002e
    }
}
