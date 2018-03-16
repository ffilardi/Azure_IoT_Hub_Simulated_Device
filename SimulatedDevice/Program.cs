namespace SimulatedDevice
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    class Program
    {
        private const string DeviceId = "<Device Id>";
        static string ConnectionString = "<Device Connection String>";

        private const double MinTemperature = 20;
        private const double MinHumidity = 60;
        private const double AlertTemperature = 30;
        private const int messageDelay = 10000;

        private static readonly Random Rand = new Random();
        private static DeviceClient _deviceClient;
        private static int _messageId = 1;

        static void Main(string[] args)
        {
            _deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString);
            _deviceClient.ProductInfo = "SimulatedDevice-CSharp";

            SendDeviceToCloudMessagesAsync();
            ReceiveCloudToDeviceMessagesAsync();

            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {
            while (true)
            {
                var currentTemperature = MinTemperature + Rand.NextDouble() * 15;
                var currentHumidity = MinHumidity + Rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    messageId = _messageId++,
                    deviceId = DeviceId,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > AlertTemperature) ? "true" : "false");

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(messageDelay);
            }
        }

        private static async void ReceiveCloudToDeviceMessagesAsync()
        {
            while (true)
            {
                Message receivedMessage = await _deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0} > Received message: {1}", DateTime.Now, JsonConvert.SerializeObject(new { body = Encoding.ASCII.GetString(receivedMessage.GetBytes()) }));
                Console.ResetColor();

                await _deviceClient.CompleteAsync(receivedMessage);
            }
        }
    }
}
