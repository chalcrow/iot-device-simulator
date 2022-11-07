
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoT.BuildingDevice
{
    class Program
    {
        //Connection string for device to cloud messaging
        private static readonly string connectionString_IoTHub = "HostName=iot-test-dev-australiasoutheast-001.azure-devices.net;DeviceId=AZ3166;SharedAccessKey=duwMHgL/SljeQpLjh5Fi1mKyLtV0ZycVfq0PVHi0pUs=";

        //Device Client
        static DeviceClient buildingDeviceClient;

        //Random Generator
        static Random random = new Random();

        //building sensor details
        const double maximumOccupancy_min = 0;
        const double maximumOccupancy_max = 100;
        static double maximumOccupancy = 3;

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.WriteLine("Press CTRL+C to stop the simulation");
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Stopping the Application....");
                cts.Cancel();
                e.Cancel = true;
            };

            buildingDeviceClient = DeviceClient.CreateFromConnectionString(connectionString_IoTHub);

            SendMessagesToIoTHub(cts.Token);

            Console.ReadLine();

        }

        private static async void SendMessagesToIoTHub(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                maximumOccupancy = GenerateSensorReading(maximumOccupancy, maximumOccupancy_min, maximumOccupancy_max);

                var json = CreateJSON(maximumOccupancy);
                var message = CreateMessage(json);
                await buildingDeviceClient.SendEventAsync(message);
                Console.WriteLine($"Sending message at {DateTime.Now} and Message : {json}");
                await Task.Delay(5000);
            }
        }

        private static double GenerateSensorReading(double currentValue, double min, double max)
        {
            //double percentage = 5; // 5%

            //// generate a new value based on the previous supplied value
            //// The new value will be calculated to be within the threshold specified by the "percentage" variable from the original number.
            //// The value will also always be within the the specified "min" and "max" values.
            //double value = currentValue * (1 + ((percentage / 100) * (2 * random.NextDouble() - 1)));

            //value = Math.Max(value, min);
            //value = Math.Min(value, max);

            //return value;

            return currentValue;
        }

        private static string CreateJSON(double occupancyData)
        {
            var workspacesData = new List<object>();

            var buildingdata = new
            {
                buildingName = "242-exhibition-melb",
                levelName = "Level-9",
                id = "W.9.027",
                maximumOccupancy = occupancyData
            };

            workspacesData.Add(buildingdata);

            var data = new
            {
                workspaces = workspacesData
            };


            return JsonConvert.SerializeObject(data);
        }

        private static Message CreateMessage(string jsonObject)
        {
            var message = new Message(Encoding.ASCII.GetBytes(jsonObject));

            // MESSAGE CONTENT TYPE
            message.ContentType = "application/json";
            message.ContentEncoding = "UTF-8";

            return message;
        }
    }
}