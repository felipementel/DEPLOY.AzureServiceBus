using Bogus;
using System.Text.Json.Serialization;

namespace DEPLOY.AzureServiceBus.API.Util
{
    public static class GenerateData
    {
        public static List<int> Numbers(int minValue, int maxValue, int quantity)
        {
            return Enumerable
                .Range(0, quantity)
                .Select(
                    _ => new Random()
                    .Next(minValue, maxValue)
                )
                .ToList();
        }


        public static List<string> Names(int quantity)
        {
            return Enumerable
                .Range(0, quantity)
                .Select(
                    _ => new Faker().Name.FullName()
                )
                .ToList();
        }

        public static List<Product> Products(int quantity)
        {
            return new Faker<Product>(locale: "pt_BR")
                .RuleFor(p => p.Version, f => 1)
                .RuleFor(p => p.MessageType, f => MessageType.Command.ToString())
                .RuleFor(p => p.SKU, f => f.Commerce.Ean13())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(p => p.Quantity, f => f.Random.Int(1, 100))
                .FinishWith((f, u) =>
                {
                    Console.WriteLine("Product generated! Id={0}", u.SKU);
                })
                .Generate(quantity);
        }

        public class Product : BaseMessage<int>
        {
            public required string SKU { get; set; }
            public required string Name { get; set; }
            public required decimal Price { get; set; }
            public required int Quantity { get; set; }
        }

        public class BaseMessage<TVersion>
        {
            public required TVersion Version { get; set; }
            public required string MessageType { get; set; }
        }

        public enum MessageType : byte
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            Command = 1,
            [JsonConverter(typeof(JsonStringEnumConverter))]
            Event = 2
        }
    }
}
