using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Virtual_Call_Project_Testing.Hubs
{
    public class MeetingHub : Hub
    {
        public async Task JoinRoom(string roomId, string userId, string role)
        {
            if (string.IsNullOrWhiteSpace(roomId))
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.Group(roomId)
                .SendAsync("UserJoined", userId, role);
        }

        public async Task SendMessage(string roomId, string userId, string message)
        {
            await Clients.Group(roomId)
                .SendAsync("ReceiveMessage", userId, message);
        }

        // WebRTC Signaling
        public async Task SendSignal(string roomId, string userId, object signal)
        {
            await Clients.OthersInGroup(roomId)
                .SendAsync("ReceiveSignal", userId, signal);
        }
    }
}
