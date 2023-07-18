using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VMWareView
{
    internal class Program
    {
      
        static void Main(string[] args)
        {
            //VMWare.GetVMinstalledpath(out string path);
            //VMWare.CreateConfigFile();
            //VMWare.CreateShortcut();
            //Citrix.createCitrixConnectionDir("Gokul");
            //Citrix.Configure();
            //ProcessCreator.CreateProcess("mstsc.exe \"C:\\Users\\Admin\\Documents\\TestRDP.rdp\"", "C:\\Windows\\System32\\mstsc.exe"); //create process 
            RDP.Configure("TestRDP");
            RDP.AutoStart("TestRDP");
        }
     
    }
}
