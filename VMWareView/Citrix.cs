using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VMWareView
{
    public class Citrix
    {
        public static string GetVersion()
        {
            //fetch version details and enterprise version is not supported

            //check for Standard
            //1) Read Registry from path CITRIX_STANDARD_INSTALL_PATH and registry key "Version" if it is there then its standard Version for x86
            string CITRIX_STANDARD_INSTALL_PATH = "Software\\Citrix\\Dazzle";

            //2) Read Registry from path CITRIX_STANDARD_INSTALL_PATH_WOW and registry key "Version" if it is there then its standard Version for X64
            string CITRIX_STANDARD_INSTALL_PATH_WOW = "Software\\Wow6432Node\\Citrix\\Dazzle";

            //return "Standard";

            //check for Enterprise
            //1) Read Registry from path CITRIX_PNAGENT_INSTALL_PATH and registry key IS PRESENT THEN ENTERPRISE Version for x86
            string CITRIX_PNAGENT_INSTALL_PATH = "Software\\Citrix\\Install\\PNAgent";
            //2) Read Registry from path CITRIX_PNAGENT_INSTALL_PATH_WOW and registry key IS PRESENT THEN ENTERPRISE Version for X64
            string CITRIX_PNAGENT_INSTALL_PATH_WOW = "Software\\Wow6432Node\\Citrix\\Install\\PNAgent";
            //return "Enterprise";

            return "Standard";
        }

        public static void Configure()
        {
            bool _bSuccessfullyConfigured = false;
            string ConnectionName = "Test";
            string ConnectionType = "XenApp";
            string shortcutLNK;
            string mActualName = "";

            if (GetVersion() == "Standard")
            {
                if (ConnectionType == "XenApp") //this is drop down value from wms
                {
                    //Citrix Name Null Check condition we have to keep here
                    if (string.IsNullOrEmpty(ConnectionName))
                    {
                        ConnectionName = "ICAConnetion";
                        shortcutLNK = "ICAConnetion.lnk";
                        mActualName = string.IsNullOrEmpty(mActualName) ? ConnectionName : null;
                    }
                    else
                    {
                        shortcutLNK = ConnectionName;
                        shortcutLNK += "_";
                        shortcutLNK += "ICAConnetion.lnk";
                        mActualName = string.IsNullOrEmpty(mActualName) ? ConnectionName : null;
                    }
                    createCitrixConnectionDir(ConnectionName);
                    _bSuccessfullyConfigured = CreateICAFile(ConnectionName);
                    _ = _bSuccessfullyConfigured == true ? createCitrixConnection(ConnectionName, ConnectionType, shortcutLNK) : true;

                }
                else
                {
                    if (string.IsNullOrEmpty(ConnectionName))
                    {
                        ConnectionName = "StoreConnection";
                        shortcutLNK = "StoreConnection.lnk";
                    }
                    else
                    {
                        shortcutLNK = ConnectionName;
                        shortcutLNK += "_";
                        shortcutLNK += "StoreConnection.lnk";
                    }
                    createCitrixConnectionDir(ConnectionName);
                    _bSuccessfullyConfigured = createCitrixConnection(ConnectionName, ConnectionType, shortcutLNK);
                }
            }
        }
        public static void createCitrixConnectionDir(string ConnectionName)
        {
            string ctxdir = VMWare.GetSpecialFolder((int)Path_Specifier.MyDocument);
            ctxdir += "\\";
            ctxdir += "wdaCtrx";
            if (!Directory.Exists(ctxdir))
            {
                Directory.CreateDirectory(ctxdir);
                createDirwithEveryoneAccess(ctxdir);
            }
            ctxdir += "\\";
            ctxdir += ConnectionName;
            if (!Directory.Exists(ctxdir))
            {
                Directory.CreateDirectory(ctxdir);
                createDirwithEveryoneAccess(ctxdir);
            }
        }

        public static bool createDirwithEveryoneAccess(string dirName)
        {
            try
            {
                // Make sure directory exists
                if (Directory.Exists(dirName) == false)
                    throw new Exception(string.Format("Directory {0} does not exist, so permissions cannot be set.", dirName));

                // Get directory access info
                DirectoryInfo dinfo = new DirectoryInfo(dirName);
                DirectorySecurity dSecurity = dinfo.GetAccessControl();

                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

                // Set the access control
                dinfo.SetAccessControl(dSecurity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CreateICAFile(string Name)
        {
            string citrixServer = "https://xd1912srv2.wat.com";
            string LaunchApp = "Notepad";
            string strICAtmp = "";
            string connectionLink = "";
            string strFilePath = getICAFile(Name);
            //create file
            using (FileStream fs = File.Create(strFilePath)) ;
            string strICA = "";
            strICA += "[WFClient]\n";
            strICA += "Version=2\n";

            string strCitrixAddrTmp = citrixServer;
            if (!strCitrixAddrTmp.Contains("http://")) //this is for http://
            {

            }
            strICAtmp = "HttpBrowserAddress";
            strICAtmp += strCitrixAddrTmp + "\n";
            //tsFQDN_to_IP removing https://
            strCitrixAddrTmp = citrixServer.Contains("https://") ? citrixServer.Substring(8) : citrixServer.Substring(7);
            if (string.IsNullOrEmpty(Convert_FQDN_IP(strCitrixAddrTmp)))
            {
                strCitrixAddrTmp = citrixServer;
            }
            LaunchApp = string.IsNullOrEmpty(LaunchApp) ? "Desktop" : LaunchApp;
            strICA += strICAtmp;
            strICA += "\r\n";
            strICA += "\r\n";
            strICA += "[ApplicationServers]\n";
            strICA += LaunchApp + "=";
            strICA += "\r\n";
            strICA += "\r\n";
            strICA += "[" + LaunchApp + "]";
            strICA += "Address" + "=" + strCitrixAddrTmp;
            strICA += "InitialProgram=#" + LaunchApp;
            strICA += "Username=Administrator";
            strICA += "ClearPassword=";
            strICA += "Domain=";
            strICA += "ClientAudio=On";
            strICA += "AudioBandwidthLimit=2";
            strICA += "TWIMode=On\r\n";
            strICA += "DesiredColor=4";
            strICA += "DesiredHRes=4294967295";
            strICA += "DesiredVRes=4294967295";
            strICA += "TransportDriver=TCP/IP";
            strICA += "WinStationDriver=ICA 3.0";
            strICA += "BrowserProtocol=HTTPonTCP";
            File.WriteAllText(strFilePath, strICA); //FileWrite 
            if (string.IsNullOrEmpty(Name))
            {
                connectionLink = "ICAConnetion.lnk";
            }
            else
            {
                connectionLink = Name;
                connectionLink += "_ICAConnetion.lnk";
            }
            string path = "C:\\Program Files (x86)\\Citrix\\ICA Client\\";
            //getCitrixInstallPath(out path); //fetch from this reg 
            path += "\\wfcrun32.exe";
            string sArgs = "\"";
            sArgs += strFilePath;
            sArgs += "\"";
            Shortcut.Create(connectionLink, path, sArgs, null, "Citrix ICA connection Link", null);
            return true;
        }

        public static string Convert_FQDN_IP(string URL)
        {
            System.Net.IPAddress[] addresses = System.Net.Dns.GetHostAddresses(URL);
            return addresses.ToList().FirstOrDefault().ToString() ?? string.Empty;
        }
        public static string getICAFile(string connectionName)
        {
            string strFilePath;
            strFilePath = VMWare.GetSpecialFolder(0x0005);
            strFilePath += "\\";
            strFilePath += "wdaCtrx";
            strFilePath += "\\";
            strFilePath += connectionName;
            strFilePath += "\\";
            strFilePath += connectionName;
            strFilePath += ".ica";
            return strFilePath;
        }

        public static bool createCitrixConnection(string Name, string CitrixPubType, string shortcutLink)
        {
            //Write JSON to File
            string ConnectionFileInfo = getConnectionInfoFile(Name);
            Json(ConnectionFileInfo);

            //Create Desktop Shortcut
            string path = getLaunchApp();
            string connpathargs = getLaunchArgs(ConnectionFileInfo);
            string iconpath;
            getCitrixInstallPath(out iconpath);
            iconpath += "Receiver.ico";
            string szlinksrt = VMWare.GetSpecialFolder((int)Path_Specifier.COMMON_DESKTOPDIRECTORY);
            szlinksrt += "\\";
            szlinksrt += shortcutLink;
            if (CitrixPubType == "XenApp")
            {
                Shortcut.Create(szlinksrt, path, connpathargs, null, "Citrix ICA connection Link", iconpath);
            }
            else
            {
                Shortcut.Create(szlinksrt, path, connpathargs, null, "Citrix Store connection Link", iconpath);
            }
            return true;
        }
        public static string getConnectionInfoFile(string Name)
        {
            string strFilePath;
            strFilePath = VMWare.GetSpecialFolder((int)Path_Specifier.MyDocument);
            strFilePath += "\\";
            strFilePath += "wdaCtrx";
            strFilePath += "\\";
            strFilePath += Name;
            strFilePath += "\\";
            strFilePath += "connectionInfo.json";
            return strFilePath;
        }

        public static void Json(string fileName)
        {
            List<CitrixData> ctxdata = new List<CitrixData>();
            ctxdata.Add(new CitrixData
            {
                connectionName = "Test",
                connectionType = "StoreFront",
                serverURL = "https://xd1912srv2.wat.com",
                logonMethod = "Explicit",
                userName = "wat\\administrator",
                password = "Wyse@12345",
                domain = "",
                storeName = "Store",
                audio = "high",
                keyPassthrough = "local",
                launchApp = "Notepad"
            });
            string json = JsonConvert.SerializeObject(ctxdata, Formatting.Indented);
            File.WriteAllText(fileName, json.Replace("[", "").Replace("]", ""));
        }

        public static string getLaunchApp()
        {
            string path = "";
            path = VMWare.GetSpecialFolder((int)Path_Specifier.PROGRAM_FILES).Replace("Program Files (x86)", "Program Files");
            path += "\\Wyse\\WDA\\Bin\\";
            path += "\\";
            path += "DtcCitrixConnectionAgent.exe";
            return path;
        }

        public static string getLaunchArgs(string connectionInfoFile)
        {
            string sArgs = "-LaunchConnection ";
            sArgs += "\"";
            sArgs += "ConnectionInfo=";
            sArgs += connectionInfoFile;
            sArgs += "\"";
            return sArgs;
        }

        public static string getCitrixInstallPath(out string icon)
        {
            //check if same as vmware enterpeise or standard
            //read registry for x86
            //1) Enterprise
            //if this will return true then use this "InstallFolder" path for x86
            string CITRIX_ENTERPRISE_INSTALL_PATH = "Software\\Citrix\\Install\\ICA Client"; //x86
            //else
            // Check for x64
            string CITRIX_ENTERPRISE_INSTALL_PATH_WOW = "Software\\Wow6432Node\\Citrix\\Install\\ICA Client";
            //read the key "InstallFolder" for fetching path

            //same as above standard
            string CITRIX_STANDARD_INSTALL_PATH = "Software\\Citrix\\Dazzle";//x86
            //x64 
            string CITRIX_STANDARD_INSTALL_PATH_WOW = "Software\\Wow6432Node\\Citrix\\Dazzle";

            //read the Key "InstallDir" for fetching the path

            return icon = "C:\\Program Files (x86)\\Citrix\\ICA Client\\SelfServicePlugin\\";
        }

        public static bool ApplyCitrixAdobeFlashSettings()
        {
            /*
             * Setting the registry in this function
             */
            string regTips = "Software\\Citrix\\ICA Client\\Keyboard Mapping\\Tips"; //HKCU
            //above reg set key =In full screen mode

            //UseFlashRemoting = key
            string flashpath = "Software\\Policies\\Citrix\\HdxMediaStreamForFlash\\Client\\PseudoContainer"; //HKCU

            //UseServerHttpCookies =Key
            string pathregkey = "Software\\Policies\\Citrix\\HdxMediaStreamForFlash\\Client\\PseudoContainer";

            //EnableServersideContentFetching
            string enablereg = "Software\\Policies\\Citrix\\HdxMediaStreamForFlash\\Client\\PseudoContainer";

            //after this we need to configure content rule using registry 
            string conterrule = "SOFTWARE\\Policies\\Citrix\\HdxMediaStreamForFlash\\Client\\PseudoContainer\\ContentURLRewritingRules";//HKCU
            //add content rule based on array so we need to add one by one using loop

            //for (size_t nIndex = 0; nIndex < contentRules.size(); nIndex++)
            //{
            //    SetRegistryString(HKCU, L"SOFTWARE\\Policies\\Citrix\\HdxMediaStreamForFlash\\Client\\PseudoContainer\\ContentURLRewritingRules", contentRules[nIndex]->sRuleName.c_str(), contentRules[nIndex]->sRuleValue.c_str());
            //}

            return true;
        }
    }

    public class CitrixData
    {
        public string connectionName { get; set; }
        public string connectionType { get; set; }
        public string serverURL { get; set; }
        public string logonMethod { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string domain { get; set; }
        public string storeName { get; set; }
        public string audio { get; set; }
        public string keyPassthrough { get; set; }
        public string launchApp { get; set; }
    }
}

