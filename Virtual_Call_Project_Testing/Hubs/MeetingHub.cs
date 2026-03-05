using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Virtual_Call_Project_Testing.Hubs
{
    public class MeetingHub : Hub
    {
        private static ConcurrentDictionary<string, string> RoomTeachers = new();
        private static ConcurrentDictionary<string, string> UserConnections = new();
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> RoomParticipants = new();
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> PendingUsers = new();

        public async Task JoinRoom(string roomId, string userId, string role)
        {
            UserConnections[userId] = Context.ConnectionId;

            // FIX: Add the user (Teacher OR Student) to the group immediately
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            if (role == "teacher")
            {
                RoomTeachers[roomId] = userId;
                RoomParticipants.TryAdd(roomId, new());
                RoomParticipants[roomId][userId] = true;

                // Refresh list for teacher
                await Clients.Caller.SendAsync("ParticipantsUpdated", RoomParticipants[roomId].Keys);
            }
        }

        public async Task RequestToJoin(string roomId, string studentUserId)
        {
            UserConnections[studentUserId] = Context.ConnectionId;
            PendingUsers.TryAdd(roomId, new());
            PendingUsers[roomId][studentUserId] = true;

            if (RoomTeachers.TryGetValue(roomId, out var teacherId) &&
                UserConnections.TryGetValue(teacherId, out var teacherConnection))
            {
                await Clients.Client(teacherConnection).SendAsync("JoinRequestReceived", studentUserId);
            }
        }

        public async Task AcceptUser(string roomId, string studentUserId)
        {
            if (PendingUsers.TryGetValue(roomId, out var pending))
                pending.TryRemove(studentUserId, out _);

            RoomParticipants.TryAdd(roomId, new());
            RoomParticipants[roomId][studentUserId] = true;

            if (UserConnections.TryGetValue(studentUserId, out var studentConn))
            {
                // Ensure student is in the group
                await Groups.AddToGroupAsync(studentConn, roomId);

                // FIX: Broadcast to the whole GROUP so the Teacher's UI also updates
                await Clients.Group(roomId).SendAsync("UserAccepted", studentUserId);
                await Clients.Group(roomId).SendAsync("ParticipantsUpdated", RoomParticipants[roomId].Keys);
            }
        }

        public async Task RejectUser(string roomId, string studentUserId)
        {
            if (PendingUsers.TryGetValue(roomId, out var pending))
                pending.TryRemove(studentUserId, out _);

            if (UserConnections.TryGetValue(studentUserId, out var studentConn))
            {
                await Clients.Client(studentConn).SendAsync("UserRejected", studentUserId);
            }
        }

        public async Task SendMessage(string roomId, string userId, string message)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", userId, message);
        }

        public async Task SendSignal(string roomId, string targetUserId, string senderUserId, object signal)
        {
            if (UserConnections.TryGetValue(targetUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveSignal", senderUserId, signal);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(user))
            {
                UserConnections.TryRemove(user, out _);
                foreach (var room in RoomParticipants)
                {
                    if (room.Value.TryRemove(user, out _))
                    {
                        await Clients.Group(room.Key).SendAsync("UserDisconnected", user);
                        await Clients.Group(room.Key).SendAsync("ParticipantsUpdated", room.Value.Keys);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}