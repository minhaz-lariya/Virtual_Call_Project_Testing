using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Virtual_Call_Project_Testing.Hubs
{
    public class MeetingHub : Hub
    {
        // roomId -> teacherUserId
        private static ConcurrentDictionary<string, string> RoomTeachers = new();

        // userId -> connectionId
        private static ConcurrentDictionary<string, string> UserConnections = new();

        // roomId -> list of participants
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> RoomParticipants = new();

        /* ==============================
           JOIN ROOM
        ============================== */
        public async Task JoinRoom(string roomId, string userId, string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            // Map userId to latest connectionId
            UserConnections[userId] = Context.ConnectionId;

            // Add participant to room
            RoomParticipants.TryAdd(roomId, new ConcurrentDictionary<string, bool>());
            RoomParticipants[roomId][userId] = true;

            Console.WriteLine($"JOIN: {userId} | {role} | {roomId}");

            if (role == "teacher")
            {
                RoomTeachers[roomId] = userId;
                Console.WriteLine($"TEACHER REGISTERED: {roomId}");
            }

            // Notify everyone in room about participants (teacher + accepted students)
            await Clients.Group(roomId)
                .SendAsync("ParticipantsUpdated", RoomParticipants[roomId].Keys);
        }

        /* ==============================
           CHAT
        ============================== */
        public async Task SendMessage(string roomId, string userId, string message)
        {
            await Clients.Group(roomId)
                .SendAsync("ReceiveMessage", userId, message);
        }

        /* ==============================
           WEBRTC SIGNALING
        ============================== */
        public async Task SendSignal(string roomId, string targetUserId, string senderUserId, object signal)
        {
            // Send the signal only to the intended recipient
            if (UserConnections.TryGetValue(targetUserId, out var targetConnection))
            {
                await Clients.Client(targetConnection)
                    .SendAsync("ReceiveSignal", senderUserId, signal);
            }
        }

        /* ==============================
           STUDENT REQUEST TO JOIN
        ============================== */
        public async Task RequestToJoin(string roomId, string studentUserId)
        {
            Console.WriteLine($"REQUEST FROM {studentUserId} FOR ROOM {roomId}");

            if (RoomTeachers.TryGetValue(roomId, out var teacherUserId))
            {
                if (UserConnections.TryGetValue(teacherUserId, out var teacherConnectionId))
                {
                    await Clients.Client(teacherConnectionId)
                        .SendAsync("JoinRequestReceived", studentUserId);
                }
            }
        }

        /* ==============================
           TEACHER ACCEPT STUDENT
        ============================== */
        public async Task AcceptUser(string roomId, string studentUserId)
        {
            if (UserConnections.TryGetValue(studentUserId, out var studentConnectionId))
            {
                // Send acceptance only to the specific student
                await Clients.Client(studentConnectionId)
                    .SendAsync("UserAccepted", studentUserId);
            }

            // Optionally update participants for everyone (teacher UI)
            if (RoomParticipants.TryGetValue(roomId, out var participants))
            {
                await Clients.Clients(RoomTeachers.Values)
                    .SendAsync("ParticipantsUpdated", participants.Keys);
            }
        }

        /* ==============================
           TEACHER REJECT STUDENT
        ============================== */
        public async Task RejectUser(string roomId, string studentUserId)
        {
            if (UserConnections.TryGetValue(studentUserId, out var studentConnectionId))
            {
                await Clients.Client(studentConnectionId)
                    .SendAsync("UserRejected", studentUserId);
            }
        }

        /* ==============================
           USER DISCONNECTED
        ============================== */
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedUser = UserConnections
                .FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

            if (!string.IsNullOrEmpty(disconnectedUser))
            {
                UserConnections.TryRemove(disconnectedUser, out _);

                // Remove from all rooms
                foreach (var room in RoomParticipants)
                {
                    if (room.Value.ContainsKey(disconnectedUser))
                    {
                        room.Value.TryRemove(disconnectedUser, out _);

                        await Clients.Group(room.Key)
                            .SendAsync("ParticipantsUpdated", room.Value.Keys);
                    }
                }

                // Remove teacher if disconnected
                foreach (var teacher in RoomTeachers)
                {
                    if (teacher.Value == disconnectedUser)
                    {
                        RoomTeachers.TryRemove(teacher.Key, out _);
                        Console.WriteLine($"TEACHER DISCONNECTED: {teacher.Key}");
                        // Optionally notify students that teacher left
                        break;
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}