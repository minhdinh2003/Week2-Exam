using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using QuanLySach.Data;
using QuanLySach.MongoModels;
using QuanLySach.Repository;
using QuanLySach.Repository.Interface;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));

builder.Services.AddScoped<IMongoCollection<Review>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(builder.Configuration["MongoDB:Database"]);
    return database.GetCollection<Review>("Reviews");
});

builder.Services.AddScoped<IUnitOfWork>(sp =>
{
    var context = sp.GetRequiredService<AppDbContext>();
    var mongoCollection = sp.GetRequiredService<IMongoCollection<Review>>();
    return new UnitOfWork(context, mongoCollection);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "QuanLySach_";
});
builder.Services.AddScoped<IRedisService, RedisCacheService>();


builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IReviewerRepository, ReviewerRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
