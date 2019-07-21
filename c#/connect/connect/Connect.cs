using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace connect
{
    class Connect
    {
        static void Main(string[] args)
        {
            Console.WriteLine("running HashPowerOrder");
            new Hpo();
            //Console.WriteLine("\n\nrunning Exchange");
            //new Exch();
            Console.ReadLine();
        }
    }

    public class ServerTime
    {
        public string serverTime { get; set; }
    }
    public class Pool
    {
        public string id { get; set; }
    }
    public class Order
    {
        public string id { get; set; }
    }
    public class OrderBooks
    {
        public List<double[]> sell { get; set; }
        public List<double[]> buy { get; set; }
    }
    
  

    public partial class DeribitCurrency
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("result")]
        public List<Result> Result { get; set; }

        [JsonProperty("usIn")]
        public long UsIn { get; set; }

        [JsonProperty("usOut")]
        public long UsOut { get; set; }

        [JsonProperty("usDiff")]
        public long UsDiff { get; set; }

        [JsonProperty("testnet")]
        public bool Testnet { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("volume")]
        public double? Volume { get; set; }

        [JsonProperty("underlying_price")]
        public double? UnderlyingPrice { get; set; }

        [JsonProperty("underlying_index")]
        public string UnderlyingIndex { get; set; }

        [JsonProperty("quote_currency")]
        public string QuoteCurrency { get; set; }

        [JsonProperty("open_interest")]
        public double? OpenInterest { get; set; }

        [JsonProperty("mid_price")]
        public double? MidPrice { get; set; }

        [JsonProperty("mark_price")]
        public double MarkPrice { get; set; }

        [JsonProperty("low")]
        public double? Low { get; set; }

        [JsonProperty("last")]
        public double? Last { get; set; }

        [JsonProperty("interest_rate")]
        public double? InterestRate { get; set; }

        [JsonProperty("instrument_name")]
        public string InstrumentName { get; set; }

        [JsonProperty("high")]
        public double? High { get; set; }

        [JsonProperty("estimated_delivery_price")]
        public double? EstimatedDeliveryPrice { get; set; }

        [JsonProperty("creation_timestamp")]
        public long CreationTimestamp { get; set; }

        [JsonProperty("bid_price")]
        public double? BidPrice { get; set; }

        [JsonProperty("base_currency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("ask_price")]
        public double? AskPrice { get; set; }
    }
}
