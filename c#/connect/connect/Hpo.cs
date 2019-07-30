using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace connect
{
    class Hpo
    {
        string dayteofexpire = "2aug19";
        string option = "P";
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static string URL_ROOT = "https://deribit.com";
        private static string ORG_ID = "";
        private static string API_KEY = "";
        private static string API_SECRET = "";

        private static string ALGORITHM = "X16R"; //algo of your order
        private static string CURRENCY = "TBTC"; //user BTC for production

        public Hpo()
        {
            Console.WriteLine("Введите дату экспирации опциона");
            dayteofexpire = Console.ReadLine();
            Console.WriteLine("Введите направление опциона P или C");
            option = Console.ReadLine();
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            NLog.LogManager.Configuration = config;

            Api api = new Api(URL_ROOT, ORG_ID, API_KEY, API_SECRET);

            // my CODE
            string pattern = @"^(\w+)-(\w+)-(\d*)-(\w?)$";
            var alllist = api.get("/api/v2/public/get_book_summary_by_currency");
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<DeribitCurrency>(alllist);
            var _list = new List<double?>();

            var rg = new Regex(@"-(\d+)-");
            var result2 = rg.Match(list.Result[0].InstrumentName).Groups[1].Value;
            var sort = new List<double?>();

            //int number;
            //string resultString = string.Join(string.Empty, Regex.Matches(list.Result[0].InstrumentName, @"-(\d+)-").OfType<Match>().Select(m => m.Groups[1].Value));
            //int.TryParse(resultString, out number);
            double? num = 0;

            string resultString = null;


            var result = from book in list.Result
                         where new Regex(pattern).Match(book.InstrumentName).Groups[2].Value == dayteofexpire.ToUpper() &&
                          new Regex(pattern).Match(book.InstrumentName).Groups[4].Value == option.ToUpper()
                         select book;

            foreach (var t in result.OrderByDescending(x=>x.InstrumentName))
            {
                num += t.OpenInterest;
                Console.WriteLine(t.InstrumentName + "  " + t.OpenInterest);
                resultString = string.Join(string.Empty, Regex.Matches(t.InstrumentName, @"-(\d+)-").OfType<Match>().Select(m => m.Groups[1].Value));
                sort.Add(Int32.Parse(resultString) * t.OpenInterest);
            }

            Console.WriteLine($"Итог по страйку {dayteofexpire.ToUpper()} [{option.ToUpper()}], равен : {num}");

            foreach (var VARIABLE in sort)
            {
                Console.WriteLine(VARIABLE);
            }

            var sum = sort.Sum();
            var total = sum / num;

            Console.WriteLine("Сумма средних : {0}", sum);
            Console.WriteLine(new string('-', 35));
            Console.WriteLine("Средняя позиция по инстументу {0}, напрвления {1}, равно =  {2}", dayteofexpire.ToUpper(), option.ToUpper(), total);
            Console.ReadLine();


            Console.Read();

            foreach (var t in list.Result)
            {
                if (!t.InstrumentName.ToUpper().Contains(dayteofexpire.ToUpper())) continue;
                var variable = t.OpenInterest;
                if (variable != null) _list.Add(variable.Value);
            }

            foreach (var VARIABLE in _list)
            {
                Console.WriteLine(VARIABLE);
            }

            Console.Read();

            var res = list.Result[0].InstrumentName.ToCharArray().Where(n => char.IsDigit(n)).ToArray();

            Console.WriteLine(res);
            Console.WriteLine(list.Result[0].InstrumentName);
            Console.Read();
















            //get server time
            string timeResponse = api.get("/api/v2/time");
            ServerTime serverTimeObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerTime>(timeResponse);
            string time = serverTimeObject.serverTime;
            Logger.Info("server time: {}", time);

            //get algo settings
            string algosResponse = api.get("/main/api/v2/mining/algorithms");
            DataSet algoObject = Newtonsoft.Json.JsonConvert.DeserializeObject<DataSet>(algosResponse);
            DataTable algoTable = algoObject.Tables["miningAlgorithms"];

            DataRow mySettings = null;
            foreach (DataRow algo in algoTable.Rows)
            {
                if (algo["algorithm"].Equals(ALGORITHM))
                {
                    mySettings = algo;
                }
            }
            Logger.Info("algo settings: {}", mySettings["algorithm"]);

            //get balance
            string accountsResponse = api.get("/main/api/v2/accounting/accounts", true, time);
            DataTable accountsArray = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(accountsResponse);

            DataRow myBalace = null;
            foreach (DataRow account in accountsArray.Rows)
            {
                if (account["currency"].Equals(CURRENCY))
                {
                    myBalace = account;
                }
            }
            Logger.Info("balance: {} {}", myBalace["balance"], CURRENCY);

            //create pool
            Dictionary<string, string> pool = new Dictionary<string, string>
            {
                { "algorithm", ALGORITHM },
                { "name", "my pool " + Guid.NewGuid().ToString() },
                { "username", "pool_username" }, //your pool username
                { "password", "x" }, //your pool password
                { "stratumHostname", "pool.host.name" }, //pool hostname
                { "stratumPort", "3456" } //pool port
            };

            string poolResponse = api.post("/main/api/v2/pool", Newtonsoft.Json.JsonConvert.SerializeObject(pool), time, false);
            Pool poolObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Pool>(poolResponse);
            string myPoolId = poolObject.id;
            Logger.Info("new pool id: {}", myPoolId);

            //create order
            Dictionary<string, string> order = new Dictionary<string, string> {
                { "algorithm", ALGORITHM },
                { "amount", (string)mySettings["minimalOrderAmount"] },
                { "displayMarketFactor", (string)mySettings["displayMarketFactor"] },
                { "limit", (string)mySettings["minSpeedLimit"] }, // GH [minSpeedLimit-maxSpeedLimit] || 0 - unlimited speed
                { "market", "EU" },
                { "marketFactor", (string)mySettings["marketFactor"] },
                { "poolId", myPoolId },
                { "price", "0.0010" }, //per BTC/GH/day
                { "type", "STANDARD" }
            };

            string newOrderResponse = api.post("/main/api/v2/hashpower/order", Newtonsoft.Json.JsonConvert.SerializeObject(order), time, true);
            Order orderObject = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(newOrderResponse);
            string myOrderId = orderObject.id;
            Logger.Info("new order id: {}", myOrderId);

            //update price and limit
            Dictionary<string, string> updateOrder = new Dictionary<string, string> {
                { "displayMarketFactor", (string)mySettings["displayMarketFactor"] },
                { "limit", "0.11" }, // GH [minSpeedLimit-maxSpeedLimit] || 0 - unlimited speed
                { "marketFactor", (string)mySettings["marketFactor"] },
                { "price", "0.00123" } //per BTC/GH/day
            };

            string updateOrderResponse = api.post("/main/api/v2/hashpower/order/" + myOrderId + "/updatePriceAndLimit", Newtonsoft.Json.JsonConvert.SerializeObject(updateOrder), time, true);
            Logger.Info("update order response: {}", updateOrderResponse);

            //delete order
            string deleteOrderResponse = api.delete("/main/api/v2/hashpower/order/" + myOrderId, time, true);
            Logger.Info("delete order response: {}", deleteOrderResponse);

            //delete pool
            string deletePoolResponse = api.delete("/main/api/v2/pool/" + myPoolId, time, true);
            Logger.Info("update pool response: {}", deletePoolResponse);
        }
    }
}

