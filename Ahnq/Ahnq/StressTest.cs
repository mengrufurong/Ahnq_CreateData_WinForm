using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NewLife.Xml;

namespace Ahnq
{
    [XmlConfigFile(@"farm.xml", 15000)]
    public class StressTest: XmlConfig<StressTest>
    {

        public string Serialnum { get; set; }
        public int DeviceCountPerFacility { get; set; }
        public int StressTestPerTimes { get; set; }
        public string RequestUrl { get; set; }
        public List<string> callbackMessage { get; set; }

        public List<Facility> Facilities { get; set; }

       

        public StressTest()
        {
            Serialnum = "00001F001";
            DeviceCountPerFacility = 3;
            StressTestPerTimes = 5;
            RequestUrl = "192.168.1.67:10099";
            Facilities = new List<Facility>();
           
        }
       
    }

    public class Facility
    {
        public string Serialnum { get; set; }
        public string FarmSerialnum { get; set; }
        public string FacilityTypeSerialnum { get; set; }
        public string Name { get; set; }
        //public string FacilityType { get; set; }
        public List<Device> Devices { get; set; }
        public Facility()
        {
            Devices = new List<Device>();
        }

    }

    public class Device
    {
        public string DeviceTypeSerialnum { get; set; }
        public string Serialnum { get; set; }
        public string FacilitySerialnum { get; set; }
        public string Name { get; set; }
        public List<DeviceValue> DeviceValues { get; set; }

        public Device()
        {
            DeviceValues=new List<DeviceValue>();
        }
    }

    public class DeviceValue
    {
        public string Serialnum { get; set; }
        //public string DeviceSerialnum { get; set; }
        public decimal ProcessedValue { get; set; }
        public string ShowValue { get; set; }
        public DateTime UpDateTime { get; set; }
        public bool IsException { get; set; }

    }


}
