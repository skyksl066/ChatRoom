using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRoom
{
    public class ChatHub : Hub
    {
        public class LoggedInfo
        {
            public string ConnectionId { get; set; }
            public string UserID { get; set; }
            public string UserName { get; set; }
            public string Browser { get; set; }
            public string Action { get; set; }
            public string System { get; set; }
            public DateTime? LastRequest { get; set; }
        }
        private static readonly List<LoggedInfo> loggedInfos = new List<LoggedInfo>();

        #region 當連線成功
        public override Task OnConnectedAsync()
        {
            loggedInfos.Add(new LoggedInfo()
            {
                ConnectionId = Context.ConnectionId,
            });
            _ = GetLoginInfo(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        #endregion

        #region 當切斷連線
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var itemToRemove = loggedInfos.Where(x => x != null && x.ConnectionId == Context.ConnectionId).FirstOrDefault();
            if (itemToRemove != null)
            {
                loggedInfos.Remove(itemToRemove);
            }
            Clients.All.SendAsync("onUserLoginOut", itemToRemove);
            return base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region 向Client要求使用者資訊
        public async Task GetLoginInfo(string ClientId)
        {
            await Clients.Client(ClientId).SendAsync("getLoginInfo", ClientId);
        }
        #endregion

        #region Client回饋使用者資訊
        public async Task Login(string UserName/*, string Action, string Browser, string System*/)
        {
            var itemToUpdate = loggedInfos.Single(x => x.ConnectionId == Context.ConnectionId);
            itemToUpdate.UserName = UserName;
            //itemToUpdate.Browser = Browser;
            //itemToUpdate.Action = Action;
            //itemToUpdate.System = System;
            itemToUpdate.LastRequest = DateTime.Now;
            await Clients.All.SendAsync("onUserLogin", itemToUpdate);
        }
        #endregion

        #region 取得再線使用者清單
        public async Task GetOnlineList()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("onlineList", loggedInfos);
        }
        #endregion

        #region 給指定使用者發訊息
        public async Task MessageTo(List<string> to, string message)
        {
            await Clients.Clients(to).SendAsync("showMessage", Context.ConnectionId, message);
        }
        #endregion

        #region 給指定使用者發檔案
        public async Task FileTo(List<string> to, string base64)
        {
            await Clients.Clients(to).SendAsync("showFile", Context.ConnectionId, base64);
        }
        #endregion

        #region 給群組發訊息
        public async Task GroupMessageTo(List<string> group, List<string> to, string message)
        {
            await Clients.Clients(to).SendAsync("groupShowMessage", group, Context.ConnectionId, message);
        }
        #endregion

        #region 給群組發檔案
        public async Task GroupFileTo(List<string> group, List<string> to, string base64)
        {
            await Clients.Clients(to).SendAsync("groupShowFile", group, Context.ConnectionId, base64);
        }
        #endregion

        #region 給全部使用者發訊息
        public async Task Broadcast(string name, string message)
        {
            await Clients.All.SendAsync("showmessage", name, message);
        }
        #endregion
    }
}
