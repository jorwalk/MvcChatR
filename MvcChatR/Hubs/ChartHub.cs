using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace MvcChatR.Hubs
{
    public class ChartHub : Hub
    {
        public async Task Hello(string a)
        {
            await Clients.All.hello(a);
        }
    }
}