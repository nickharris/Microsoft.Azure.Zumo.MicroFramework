using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.Seeed;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.Net;

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

        private GT.Timer timer = new GT.Timer(5000); 
         
        void ProgramStarted()
        {
            // Event that fires when a measurement is ready
            temperatureHumidity.MeasurementComplete += new TemperatureHumidity.MeasurementCompleteEventHandler(temperatureHumidity_MeasurementComplete);
            ethernet_J11D.Interface.NetworkAddressChanged += new NetworkInterfaceExtension.NetworkAddressChangedEventHandler(Interface_NetworkAddressChanged);
            ethernet_J11D.DebugPrintEnabled = true;
            //Open Network
            
            ethernet_J11D.Interface.Open();
            if (!ethernet_J11D.Interface.IsActivated)
            {
                
                NetworkInterfaceExtension.AssignNetworkingStackTo(ethernet_J11D.Interface);
            }
            ethernet_J11D.Interface.NetworkInterface.EnableDhcp();
            ethernet_J11D.Interface.NetworkInterface.EnableDynamicDns();
        }
                
        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {            
            if (!timer.IsRunning)
            {
                timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
                timer.Start();
            }
        }

        void timer_Tick(GT.Timer timer)
        {          
            temperatureHumidity.RequestMeasurement();
        }

        void temperatureHumidity_MeasurementComplete(TemperatureHumidity sender, double temperature, double relativeHumidity)
        {
            //may as well take a light reading as well
            double lightPercentage = lightSensor.ReadLightSensorPercentage();

            if (this.ethernet_J11D.Interface.IsOpen)
            {                
                //create a new sensor reading and set the values
                var reading = new SensorReading()
                {
                    SensorID = "nicks-office", //can you believe i have to share my office with http://ntotten.com :)
                    Temp = temperature,
                    Humidity = relativeHumidity,
                    Light = lightPercentage,
                    DateAdded = DateTime.UtcNow
                };
                
                try
                {
                    //insert into the mobile service
                    var json = MobileService.GetTable("SensorReading").Insert(reading);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }

                Debug.Print("S:" + DateTime.Now.ToString());
            }
            else
            {
                Debug.Print("Check connection");
            }
        } 
    }
}
