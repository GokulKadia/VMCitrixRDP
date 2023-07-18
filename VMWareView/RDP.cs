using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMWareView
{
    public class RDP
    {
        /// <summary>
        /// Configure function will first entry point of the RDP Settings
        /// </summary>
        public static void Configure(string connName)
        {
            CreateRDPFile(connName);
        }

        /// <summary>
        /// In this function We will create the RDP file in Document Folder of admin
        /// second step will assing payload data to dictionary that is hard coded right now 
        /// will write that data to rdp file which we create in 1 step
        /// creating hortcuts based on os type
        /// last creating default shortcut from mstsc.exe 
        /// </summary>
        public static bool CreateRDPFile(string rdpFile)
        {
            string sRDPFile = "";
            string rdFile = rdpFile + ".rdp";
            string rdpFileCreatepath = VMWare.GetSpecialFolder((int)Path_Specifier.MyDocument);
            sRDPFile = string.IsNullOrEmpty(rdpFileCreatepath) ? "C:\\Users\\Public\\Documents\\" + rdFile : rdpFileCreatepath + "\\" + rdFile;
            using (FileStream fs = File.Create(sRDPFile)) ;
            Dictionary<dictProp, string> dictRDPData = new Dictionary<dictProp, string>();
            dictRDPData = RDPPropData();
            StreamWriter objSW = new StreamWriter(sRDPFile);
            dictRDPData.ToList().ForEach(x =>
            {
                objSW.WriteLine(x.Key.KeyName+x.Key.DataType+x.Value);
            });
            objSW.Close();

            //If Os Version < WES8S then create publicDesktopIconShortCut           
            if (getthinOs() == "WE8S")
            {               
               CreateShortcut(rdpFile, sRDPFile, (int)Path_Specifier.COMMON_DESKTOPDIRECTORY, "Remote Desktop Connection");
            }
            else
            {                
                CreateShortcut(rdpFile, sRDPFile, (int)Path_Specifier.DESKTOPDIRECTORY, "Remote Desktop Connection");
            }

            //Creating default Icon on Desktops
            DeleteDefaultRDPShortcuts();
            CreateDefaultRDPShortcuts();

            return true;
        }
               
        /// <summary>
        /// this function will create the shortcut
        /// </summary>
        public static void CreateShortcut(string ConnName, string DesktopPath, int flag, string desc)
        {
            string rdpFileCreatepath = VMWare.GetSpecialFolder((int)Path_Specifier.COMMON_DESKTOPDIRECTORY);
            rdpFileCreatepath += "\\";
            rdpFileCreatepath += ConnName + ".lnk";
            Shortcut.Create(rdpFileCreatepath, DesktopPath, null, null, desc, null);
        }
        /// <summary>
        /// this function will fetch os type from reg key WNT OS
        /// </summary>
        /// <returns></returns>
        public static string getthinOs()
        {
            //read registry path
            string regPath = "SYSTEM\\CurrentControlSet\\Control\\WNT";
            var regRead = Registry.LocalMachine.OpenSubKey(regPath);
            return regRead.GetValue("OS").ToString();
        }
        public static bool DeleteDefaultRDPShortcuts()
        {
            return getthinOs() == "WES2009" ? DeleteDesktopShortcut("Span Remote Desktop Connection.lnk") : DeleteDesktopShortcut("Remote Desktop Connection.lnk");
        }
        public static bool DeleteDesktopShortcut(string rdpFilename)
        {
            string strPath = VMWare.GetSpecialFolder((int)Path_Specifier.DESKTOPDIRECTORY);
            strPath += "\\";
            strPath += rdpFilename;
            System.IO.File.Delete(strPath);
            strPath = VMWare.GetSpecialFolder((int)Path_Specifier.COMMON_DESKTOPDIRECTORY);
            strPath += "\\";
            strPath += rdpFilename;
            System.IO.File.Delete(strPath);
            return true;
        }
        public static bool CreateDefaultRDPShortcuts()
        {
            string sysPath = VMWare.GetSpecialFolder((int)Path_Specifier.System_Path);
            sysPath += "\\mstsc.exe";
            if (getthinOs() == "WES2009")
            {
                string rdpFileCreatepath = VMWare.GetSpecialFolder((int)Path_Specifier.COMMON_DESKTOPDIRECTORY);
                rdpFileCreatepath += "\\";
                rdpFileCreatepath += "Span Remote Desktop Connection.lnk";
                Shortcut.Create(rdpFileCreatepath, sysPath, null, null, "Span Remote Desktop Connection", null);
            }
            else
            {
                string rdpFileCreatepath = VMWare.GetSpecialFolder((int)Path_Specifier.COMMON_DESKTOPDIRECTORY);
                rdpFileCreatepath += "\\";
                rdpFileCreatepath += "Remote Desktop Connection.lnk";
                Shortcut.Create(rdpFileCreatepath, sysPath, null, null, "Remote Desktop Connection", null);
            }
            return true;
        }
        public static Dictionary<dictProp, string> RDPPropData()
        {
            Dictionary<dictProp, string> rdData = new Dictionary<dictProp, string>();
            rdData.Add(new dictProp(){KeyName="allow desktop composition",DataType=":i:"}, "1");
            rdData.Add(new dictProp() { KeyName = "allow font smoothing", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "audiocapturemode", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "audiomode", DataType = ":i:" }, "2");
            rdData.Add(new dictProp() { KeyName = "authentication level", DataType = ":i:" }, "2");
            rdData.Add(new dictProp() { KeyName = "autoreconnection enabled", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "autostart", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "bitmapcachepersistenable", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "compression", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "devicestoredirect", DataType = ":s:" }, "*");
            rdData.Add(new dictProp() { KeyName = "disable cursor setting", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "disable full window drag", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "disable menu anims", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "disable themes", DataType = ":i:" }, "0");
            rdData.Add(new dictProp() { KeyName = "disable wallpaper", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "displayconnectionbar", DataType = ":s:" }, "1");
            rdData.Add(new dictProp() { KeyName = "drivestoredirect", DataType = ":i:" }, "*");
            rdData.Add(new dictProp() { KeyName = "enableworkspacereconnect", DataType = "i" }, "1");
            rdData.Add(new dictProp() { KeyName = "full address", DataType = ":s:" }, "100.106.153.135");
            rdData.Add(new dictProp() { KeyName = "gatewayprofileusagemethod", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectclipboard", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectcomports", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectdirectx", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectdrives", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectposdevices", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectprinters", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "redirectsmartcards", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "screen mode id", DataType = ":i:" }, "2");
            rdData.Add(new dictProp() { KeyName = "usbdevicestoredirect", DataType = ":s:" }, "*");
            rdData.Add(new dictProp() { KeyName = "use multimon", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "use redirection server name", DataType = ":i:" }, "1");
            rdData.Add(new dictProp() { KeyName = "username", DataType = ":s:" }, "Admin");
            rdData.Add(new dictProp() { KeyName = "videoplaybackmode", DataType = ":i:" }, "1");
            return rdData;
        }

        public static void AutoStart(string rdpFile)
        {
            string sRDPFile = "";
            string rdFile = rdpFile + ".rdp";
            //null check for rdfile here we can keep
            string rdpFileCreatepath = VMWare.GetSpecialFolder((int)Path_Specifier.MyDocument);
            rdpFileCreatepath += "\\";
            rdpFileCreatepath += rdFile;
            ProcessCreator.CreateProcess($"mstsc.exe \"{rdpFileCreatepath}\"", "C:\\Windows\\System32\\mstsc.exe"); //create process 
        }
    }
    public class dictProp
    {
        public string KeyName { get; set; }
        public string DataType { get; set; }
    }
}
