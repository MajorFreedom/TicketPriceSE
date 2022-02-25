using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DocsVision.Platform.StorageServer.Extensibility;
using Newtonsoft.Json;

namespace TicketPriceSE
{
	class TicketPrice : StorageServerExtension
	{
		[ExtensionMethod]
		public static string getContent(string url)
		{
			HttpWebRequest request =
			(HttpWebRequest)WebRequest.Create(url);

			request.Method = "GET";
			request.Accept = "application/json";
			request.UserAgent = "Mozilla/5.0 ....";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			StreamReader reader = new StreamReader(response.GetResponseStream());
			StringBuilder output = new StringBuilder();
			output.Append(reader.ReadToEnd());
			response.Close();
			return output.ToString();
		}

		[ExtensionMethod]
		public decimal MinTicketPrice(string date, string depCode, string destCode)
		{
			decimal minValue;
			DateTime dateTime = Convert.ToDateTime(date);
			string url = string.Format(@"http://min-prices.aviasales.ru/calendar_preload?origin={0}&destination={1}&depart_date={2}&one_way=true",
										depCode, destCode, date);

			string request = getContent(url);
			Price ticketsData = JsonConvert.DeserializeObject<Price>(request);


			List<Ticket> destinationTickets = ticketsData.best_prices
				.FindAll(x => x.destination == destCode && (x.depart_date == dateTime));
	
			destinationTickets.AddRange(ticketsData.current_depart_date_prices
				.FindAll(x => x.destination == destCode && (x.depart_date == dateTime)));

			if (destinationTickets.Count == 0) return 0;

			minValue = destinationTickets.Select(x => x.value).Min();

			return minValue;
		}

		[ExtensionMethod]
		public decimal SumTicketPrice(string from, string to, string code)
		{
			decimal sum = MinTicketPrice(from, "LED", code) + MinTicketPrice(to, code, "LED");
			return sum;
		}

	}
	public class Price
	{
		public List<Ticket> current_depart_date_prices { get; set; }
		public List<Ticket> best_prices { get; set; }
	}


	public class Ticket
	{
		public string destination { get; set; }
		public DateTime depart_date { get; set; }
		public decimal value { get; set; }
	}
}
