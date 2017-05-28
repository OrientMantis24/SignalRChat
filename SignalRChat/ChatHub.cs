using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using System.Linq;
using SignalRChat.Models;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        #region Data

        static List<UserDetails> ConnectedUsers = new List<UserDetails>();
        static List<MessageDetails> CurrentMessages = new List<MessageDetails>();

        #endregion

        #region Methods

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetails { ConnectionId = id, Username = userName,  });
            }

            // send to caller
            Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessages);

            // send to all except caller client
            Clients.AllExcept(id).onNewUserConnected(id, userName);
        }

        public void SendMessageToAll(string userName, string message)
        {
            var dateNow = Convert.ToString(DateTime.Now);
            // store last 100 messages in cache
            AddMessageinCache(userName, message, dateNow);

            // Broad cast message
            Clients.All.messageReceived(userName, message, dateNow);
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.Username);

            }

            return base.OnDisconnected(stopCalled);
        }

        private void AddMessageinCache(string userName, string message, string date)
        {
            CurrentMessages.Add(new MessageDetails { Username = userName, Message = message, Date = date});

            //Add to database

            if (CurrentMessages.Count > 100)
                CurrentMessages.RemoveAt(0);
        }

        #endregion
    }
}