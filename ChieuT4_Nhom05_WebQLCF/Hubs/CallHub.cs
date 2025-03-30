using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using ChieuT4_Nhom05_WebQLCF.Models;

namespace ChieuT4_Nhom05_WebQLCF.Hubs
{
    [Authorize(Roles = "Customer, Employee")]
    public class CallHub : Hub
    {
        private static ConcurrentDictionary<string, List<string>> users = new ConcurrentDictionary<string, List<string>>();
        private static ConcurrentDictionary<string, bool> employeeStatus = new ConcurrentDictionary<string, bool>();
        private static ConcurrentQueue<string> employeeQueue = new ConcurrentQueue<string>();
        private static int activeUserCount = 0;

        private static readonly ConcurrentDictionary<string, ConcurrentBag<string>> Rooms = new ConcurrentDictionary<string, ConcurrentBag<string>>();
        private static ConcurrentDictionary<string, bool> callingUsers = new ConcurrentDictionary<string, bool>();

        public override async Task OnConnectedAsync()
        {
            // Only proceed if user is authenticated
            if (Context.User?.Identity?.IsAuthenticated == true && Context.User.Identity.Name != null)
            {
                Interlocked.Increment(ref activeUserCount);
                string userName = Context.User.Identity.Name;

                // Store the connection ID under the username
                users.AddOrUpdate(userName,
                new List<string> { Context.ConnectionId },
                (key, existingList) => existingList ?? new List<string> { Context.ConnectionId });

                await Clients.All.SendAsync("UpdateUserCount", activeUserCount);
            }
            else
            {
                Console.WriteLine("User is not authenticated or username is null.");
                // Handle the case where the user is not authenticated
                // This prevents adding to users list for unauthenticated users.
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Lấy username của người dùng dựa trên ConnectionId
            string userName = users.FirstOrDefault(x => x.Value.Contains(Context.ConnectionId)).Key;

            if (!string.IsNullOrEmpty(userName) && users.ContainsKey(userName))
            {
                // Loại bỏ ConnectionId khỏi danh sách users
                users.AddOrUpdate(userName,
                    new List<string>(),
                    (key, existingList) =>
                    {
                        existingList.Remove(Context.ConnectionId);
                        return existingList.Count == 0 ? null : existingList;
                    });

                Console.WriteLine($"User {userName} has been removed from the list.");

                // Kiểm tra nếu người dùng là nhân viên
                if (employeeStatus.ContainsKey(userName))
                {
                    // Cập nhật trạng thái nhân viên là rảnh
                    employeeStatus[userName] = false;
                    Console.WriteLine($"Employee {userName} is now set to available.");
                }
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Customer");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Employee");

            // Giảm số lượng người dùng hoạt động
            Interlocked.Decrement(ref activeUserCount);
            await Clients.All.SendAsync("UpdateUserCount", activeUserCount);

            // Gọi base method
            await base.OnDisconnectedAsync(exception);
        }

        public string GetUserRole()
        {
            if (Context.User.IsInRole(SD.Role_Customer)) return SD.Role_Customer;
            if (Context.User.IsInRole(SD.Role_Employee)) return SD.Role_Employee;
            if (Context.User.IsInRole(SD.Role_Admin)) return SD.Role_Admin;
            return "Unknown";
        }
        public async Task AddUserToGroup(string fullName)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                users.AddOrUpdate(fullName,
                    new List<string> { Context.ConnectionId },
                    (key, existingList) =>
                    {
                        existingList?.Add(Context.ConnectionId);
                        return existingList ?? new List<string> { Context.ConnectionId };
                    });

                string role = GetUserRole();
                if (role == SD.Role_Customer)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_Customer);
                }
                else if (role == SD.Role_Employee)
                {
                    if (!employeeQueue.Contains(fullName))
                    {
                        employeeQueue.Enqueue(fullName);
                        employeeStatus.TryAdd(fullName, false);
                    }
                    await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_Employee);
                }
                else if (role == SD.Role_Admin)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_Admin);
                }
                else
                {
                    Console.WriteLine($"{fullName} không thuộc nhóm được hỗ trợ.");
                }
            }
            else
            {
                Console.WriteLine($"{fullName} chưa đăng nhập.");
            }
        }

        public async Task JoinRoom(string roomId)
        {
            // Add the connection to the room
            Rooms.AddOrUpdate(roomId, new ConcurrentBag<string> { Context.ConnectionId }, (key, bag) =>
            {
                bag.Add(Context.ConnectionId);
                return bag;
            });

            // Notify other users in the room
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("MemberJoined", Context.ConnectionId);
            Console.WriteLine($"User {Context.ConnectionId} joined room {roomId}");
        }

        // When a user sends a signal (offer, answer, or candidate)
        public async Task SendSignal(string signal, string targetConnectionId)
        {
            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", signal, Context.ConnectionId);
                Console.WriteLine($"Signal sent from {Context.ConnectionId} to {targetConnectionId}");
            }
        }

        // When a user leaves the room
        public async Task LeaveRoom(string roomId)
        {
            if (Rooms.TryGetValue(roomId, out var connections))
            {
                // Remove the connection from the room
                connections.TryTake(out string _);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("MemberLeft", Context.ConnectionId);
                Console.WriteLine($"User {Context.ConnectionId} left room {roomId}");
            }
        }


        public async Task CallEmployeesByCustomer(string callerFullName)
        {
            if (!callingUsers.ContainsKey(callerFullName))
            {
                callingUsers[callerFullName] = false;  // Đánh dấu khách hàng đang gọi
            }
            // Tạo danh sách khách hàng với trạng thái
            var connectedUsers = callingUsers
                .Select(c => new { Name = c.Key, IsBusy = c.Value })
                .ToList();

            // Gửi danh sách người gọi tới tất cả nhân viên
            await Clients.Group(SD.Role_Employee).SendAsync("ReceiveConnectedUsers", connectedUsers);
        }

        // Nhân viên nhận cuộc gọi
        public async Task AcceptCall(string callerName)
        {

            if (callingUsers.ContainsKey(callerName))
            {
                callingUsers[callerName] = true; // Đặt trạng thái khách hàng thành "Bận"
            }

            if (users.TryGetValue(callerName, out var connectionIds))
            {
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("CallAccepted");
                }
            }

            // Cập nhật danh sách khách hàng tới nhân viên
            var connectedUsers = callingUsers
                .Select(c => new { Name = c.Key, IsBusy = c.Value })
                .ToList();

            // Cập nhật lại danh sách khách hàng đang gọi
            //var connectedUsers = callingUsers.Keys.ToList();
            await Clients.Group(SD.Role_Employee).SendAsync("ReceiveConnectedUsers", connectedUsers);
        }

        // Kết thúc cuộc gọi
        public async Task EndCall(string callerName)
        {

            // Xóa khách hàng khỏi danh sách gọi
            if (callingUsers.ContainsKey(callerName))
            {
                callingUsers.TryRemove(callerName, out _);
            }

            var connectedUsers = callingUsers
            .Select(c => new { Name = c.Key, IsBusy = c.Value })
            .ToList();

            // Cập nhật danh sách khách hàng tới nhân viên
            //var connectedUsers = callingUsers.Keys.ToList();
            await Clients.Group(SD.Role_Employee).SendAsync("ReceiveConnectedUsers", connectedUsers);

            if (users.TryGetValue(callerName, out var connectionIds))
            {
                foreach (var connectionId in connectionIds)
                {
                    await Clients.Client(connectionId).SendAsync("CallEnded");
                }
            }
        }

        // Phương thức tắt/mở camera
        public async Task UpdateDeviceState(string state)
        {
            var connectionId = Context.ConnectionId;
            var roomId = Rooms.FirstOrDefault(r => r.Value.Contains(connectionId)).Key;

            if (roomId != null)
            {
                // Gửi tín hiệu tới tất cả thành viên trong phòng, kèm theo ID của người gửi
                await Clients.Group(roomId).SendAsync("DeviceStateUpdated", state, connectionId);
            }
        }

        public async Task SendOffer(string connectionId, string offer)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        // Gửi SDP Answer từ người nhận đến người gọi
        public async Task SendAnswer(string connectionId, string answer)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        // Gửi ICE Candidate
        public async Task SendIceCandidate(string connectionId, string candidate)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        public async Task GetConnectedUsers()
        {
            var connectedUsers = users.Keys.ToList();
            await Clients.Caller.SendAsync("ReceiveConnectedUsers", connectedUsers);
        }

        public async Task GetCallList()
        {
            // Tạo danh sách người gọi với trạng thái hiện tại
            var connectedUsers = callingUsers
                .Select(c => new { Name = c.Key, IsBusy = c.Value })
                .ToList();

            // Gửi danh sách tới client yêu cầu
            await Clients.Group(SD.Role_Employee).SendAsync("ReceiveConnectedUsers", connectedUsers);
        }


        public async Task SendMessage(string message)
        {
            // Lấy tên người dùng gửi tin nhắn
            string senderFullName = Context.User.Identity.Name;

            // Gọi AddUserToGroup để đảm bảo người dùng được thêm vào nhóm tương ứng
            await AddUserToGroup(senderFullName);

            // Lấy vai trò của người gửi
            var senderRole = GetUserRole(); // Sử dụng phương thức GetUserRole() đã được định nghĩa trước

            // Gửi tin nhắn đến các nhóm tương ứng
            if (senderRole == SD.Role_Customer)
            {
                await Clients.Group(SD.Role_Employee).SendAsync("ReceiveMessage", senderFullName, message);
            }
            else if (senderRole == SD.Role_Employee)
            {
                await Clients.Group(SD.Role_Customer).SendAsync("ReceiveMessage", senderFullName, message);
            }
            else if (senderRole == SD.Role_Admin)
            {
                // Admin có thể gửi tin nhắn đến cả Customer và Employee
                await Clients.Group(SD.Role_Employee).SendAsync("ReceiveMessage", senderFullName, message);
                await Clients.Group(SD.Role_Customer).SendAsync("ReceiveMessage", senderFullName, message);
            }
            else
            {
                Console.WriteLine("Unknown role; cannot send message.");
            }
        }
    }
}



