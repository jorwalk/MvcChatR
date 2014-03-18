using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace MvcChatR.Hubs
{
    public interface IUserIdProvider
    {
        string GetUserId(IRequest request);
    }

    public class ChatHub : Hub
    {
        private static readonly ConnectionMapping<string> Connections =
            new ConnectionMapping<string>();
        
        //
        // Calling client methods

        // All connected clients.
        public async Task All(string name, string message)
        {
            await Clients.All.addNewMessageToPage(name, message);
            await Clients.Caller.notifyMessageSent();
        }
        // Display first sign on
        public async Task Starting(string name)
        {
            const string message = "New user";
            await Clients.All.addNewMessageToPage(name, message);

        }
        // A specific client identified by connection ID.
        public async Task Client(string userId, string message)
        {
            var name = Context.User.Identity.Name;

            using (var db = new UserContext())
            {
                var user = db.Users.Find(userId);
                if (user == null)
                {
                    await Clients.Caller.showErrorMessage("Could not find that user.");
                }
                else
                {
                    db.Entry(user)
                        .Collection(u => u.Connections)
                        .Query()
                        .Where(c => c.Connected == true)
                        .Load();

                    if (user.Connections == null)
                    {
                        await Clients.Caller.showErrorMessage("The user is no longer connected.");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            await Clients.Client(connection.ConnectionId)
                                .addChatMessage(name + ": " + message);
                        }
                    }
                }
            }
        }
        // All connected clients except the specified clients, identified by connection ID.
        public async Task AllExcept(string name, string message, string connectionId1)
        {
            await Clients.AllExcept(connectionId1).addNewMessageToPage(name, message);
        }
        // All connected clients in a specified group.
        public async Task Group(string name, string message, string groupname)
        {
           await Clients.Group(groupname).addNewMessageToPage(name, message);
        }

        // managing group membership
        public async Task JoinGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
            await Clients.Group(groupName).addNewGroupMessageToPage(Context.ConnectionId + " added to group");
        }
        public async Task LeaveGroup(string groupName)
        {
            await Groups.Remove(Context.ConnectionId, groupName);
            await Clients.Group(groupName).addContosoChatMessageToPage(Context.ConnectionId + " removed from group");
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

    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
    }

    public class User
    {
        [Key]
        public string UserName { get; set; }
        public ICollection<Connection> Connections { get; set; }
    }

    public class Connection
    {
        public string ConnectionId { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }

}