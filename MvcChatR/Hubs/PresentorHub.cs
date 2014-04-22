using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace MvcChatR.Hubs
{
    public class PresentorHub : Hub
    {
        public interface IUserIdProvider
        {
            string GetUserId(IRequest request);
        }
        private static readonly ConnectionMapping<string> Connections =
            new ConnectionMapping<string>();

        // All connected clients.
        public async Task All(string name, string message)
        {
            await Clients.All.addNewMessageToPage(name, message);
        }
        // Display start sign on message
        public async Task Starting(string name)
        {
            const string message = "New user";
            await Clients.All.addNewMessageToPage(name, message);

        }
        //
        // Calling Fathom Client Forward
        public async Task FathomForward()
        {
            await Clients.All.fathomForward();
        }
        //
        // Calling Fathom Client Backward
        public async Task FathomBackward()
        {
            await Clients.All.fathomBackword();
        }


        //
        // OVERRIDES
        public override Task OnConnected()
        {
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            var version = Context.QueryString["version"];
            if (version != "1.0")
            {
                Clients.Caller.notifyWrongVersion();
            }

            var name = Context.User.Identity.Name;
            // create 
            using (var db = new UserContext())
            {
                var user = db.Users
                    .Include(u => u.Connections)
                    .SingleOrDefault(u => u.UserName == name);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = name,
                        Connections = new List<Connection>()
                    };
                    db.Users.Add(user);
                }

                user.Connections.Add(new Connection
                {
                    ConnectionId = Context.ConnectionId,
                    UserAgent = Context.Request.Headers["User-Agent"],
                    Connected = true
                });
                db.SaveChanges();
            }
            Connections.Add(name, Context.ConnectionId);
            return base.OnConnected();
        }
        public override Task OnDisconnected()
        {
            var name = Context.User.Identity.Name;
            using (var db = new UserContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.Connected = false;
                db.SaveChanges();
            }

            Connections.Remove(name, Context.ConnectionId);
            return base.OnDisconnected();
        }
        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;

            if (!Connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                Connections.Add(name, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }

    public class UserContextPresentor : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
    }

    public class UserPresentor
    {
        [Key]
        public string UserName { get; set; }
        public ICollection<ConnectionPresentor> ConnectionsPresentors { get; set; }
    }

    public class ConnectionPresentor
    {
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }

}
