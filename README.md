Microsoft.Azure.Zumo.MicroFramework
===================================



**UNOFFICIAL** partial port of the Windows Azure Mobile Services client to the .NET MicroFramework



Usage:

Create an entity that will be used as a DTO between your Device and Mobile Service

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


Create a new instance of the MobileServiceClient with this you will be able to insert directly to your mobile service

    using Microsoft.WindowsAzure.MobileServices;
     
    namespace SensorDemoMobileServices
    {
        public partial class Program
        {
            //TODO: add your mobile service URI and app key below from the Windows Azure Portal https://manage.windowsazure.com 
           public static MobileServiceClient MobileService = new MobileServiceClient(
               new Uri("http://<your Windows Azure Mobile Service subdomain >.azure-mobile.net/"),
               "<your Windows Azure Mobile Service App Key> "
           );  
     
          ....
        }
    }

New up an instance of your DTO and call GetTable and Insert on the MobileServiceClient

   //create a new sensor reading and set the values
    var reading = new SensorReading()
    {
        SensorID = "nicks-office", //can you believe i have to share my office with http://ntotten.com ?
        Temp = temperature,
        Humidity = relativeHumidity,
        Light = lightPercentage,
        DateAdded = DateTime.UtcNow
    };

    //insert into your Windows Azure Mobile Service and your done!
    var json = MobileService.GetTable("SensorReading").Insert(reading);
