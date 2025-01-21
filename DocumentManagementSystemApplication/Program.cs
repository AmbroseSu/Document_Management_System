using DataAccess;
using DocumentManagementSystemApplication.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentManagementSystemDbContext>();
    context.EnsurePgCryptoExtension();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UsePermissionMiddleware();
app.MapControllers();

app.Run();