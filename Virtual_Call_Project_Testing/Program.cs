using Microsoft.EntityFrameworkCore;
using Virtual_Call_Project_Testing.ApplicationContext;
using Virtual_Call_Project_Testing.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(o => o.UseSqlServer("Data Source=.;Initial Catalog=VirtualClassRoomDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"));

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

app.UseRouting();

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.MapControllers(); // ✅ ADD THIS

app.MapHub<MeetingHub>("/meetingHub");

app.Run();