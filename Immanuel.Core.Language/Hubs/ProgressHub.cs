using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Immanuel.Core.Language.Hubs
{
    public class ProgressHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SendLearningProgress(string clientid, short prog, string message, string prgfile)
        {
            await Clients.Group(clientid).SendAsync("learningprogress", prog, message, prgfile);
        }

        public async Task join(string clientid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, clientid);
            await Clients.Group(clientid).SendAsync("joined", "Joined Successfull...");
        }
    }
}
