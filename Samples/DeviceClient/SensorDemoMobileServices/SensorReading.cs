using System;
using Microsoft.SPOT;
using Microsoft.Azure.Zumo.MicroFramework.Core;

namespace SensorDemoMobileServices
{
    public class SensorReading : IMobileServiceEntity
    {
            public int Id { get; set; }
            public string SensorID { get; set; }
            public double Temp { get; set; }
            public double Humidity { get; set; }
            public double Light { get; set; }
            public DateTime DateAdded { get; set; }
    }
}
