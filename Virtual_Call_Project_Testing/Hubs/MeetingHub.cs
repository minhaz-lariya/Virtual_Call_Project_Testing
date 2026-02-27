using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Virtual_Call_Project_Testing.Hubs
{
    public class MeetingHub : Hub
    {
        // Store teacher connection per room
        private static ConcurrentDictionary<string, string> RoomTeachers = new();

        /* ---------------- JOIN ROOM ---------------- */
        public async Task JoinRoom(string roomId, string userId, string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            Console.WriteLine($"JOIN: {userId} | {role} | {roomId}");

            if (role == "teacher")
            {
                RoomTeachers[roomId] = Context.ConnectionId;
                Console.WriteLine($"TEACHER REGISTERED: {roomId}");
            }
        }

        /* ---------------- CHAT ---------------- */
        public async Task SendMessage(string roomId, string userId, string message)
        {
            await Clients.Group(roomId)
                .SendAsync("ReceiveMessage", userId, message);
        }

        /* ---------------- WEBRTC SIGNALING ---------------- */
        public async Task SendSignal(string roomId, string userId, object signal)
        {
            await Clients.OthersInGroup(roomId)
                .SendAsync("ReceiveSignal", userId, signal);
        }

        /* ---------------- STUDENT REQUEST ---------------- */
        public async Task RequestToJoin(string roomId, string userId)
        {
            if (RoomTeachers.TryGetValue(roomId, out var teacherConnectionId))
            {
                await Clients.Client(teacherConnectionId)
                    .SendAsync("JoinRequestReceived", userId);
            }
        }

        /* ---------------- TEACHER ACCEPT ---------------- */
        public async Task AcceptUser(string roomId, string studentUserId)
        {
            await Clients.Group(roomId)
                .SendAsync("UserAccepted", studentUserId);
        }

        /* ---------------- CLEANUP ---------------- */
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var item in RoomTeachers)
            {
                if (item.Value == Context.ConnectionId)
                {
                    RoomTeachers.TryRemove(item.Key, out _);
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}