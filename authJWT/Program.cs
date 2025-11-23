using authJWT.Connection;
using authJWT.Interfaces;
using Microsoft.EntityFrameworkCore;
using authJWT.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextDb>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbJWT")), ServiceLifetime.Scoped);

builder.Services.AddSingleton<JwtService>();

// Register Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use JWT Middleware
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();
