using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Aukcija.Hubs
{
    public class DisplayHub : Hub
    {
        public void Send(string submitValue, string currency, string currencyValue, string id)
        {
            int tokens = int.Parse(submitValue);
            decimal currentP = decimal.Parse(currencyValue);

            currentP *= tokens;
            Clients.All.addNewMessageToPage(currentP.ToString("0.##"), currency, id);
        }

        public void sendForTable(string myEmail, string submitValue)
        {
            string timestamp = DateTime.Now.ToString();
            Clients.All.addNewItemToTable(myEmail, submitValue, timestamp);
        }
    }
}