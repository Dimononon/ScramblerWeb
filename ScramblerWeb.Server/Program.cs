using Services.Keys;
using Services.Scramblers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<ScramblerXOR>();
builder.Services.AddTransient<ScramblerBlockPermutation>();
builder.Services.AddTransient<ScramblerCaesar>();
builder.Services.AddTransient<IKeyGenerator, KeyGenerator>();
builder.Services.AddTransient<Func<ScramblerType, IScrambler>>(serviceProvider => type =>
{
    switch (type)
    {
        case ScramblerType.XOR:
            return serviceProvider.GetRequiredService<ScramblerXOR>();
        case ScramblerType.BlockPermutation:
            return serviceProvider.GetRequiredService<ScramblerBlockPermutation>();
        case ScramblerType.Caesar:
            return serviceProvider.GetRequiredService<ScramblerCaesar>();
        default:
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid scrambler type");
    }
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
