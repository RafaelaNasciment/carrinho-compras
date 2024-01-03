using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

#region REDIS
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString"); //Get the value from appsettings.json
});
#endregion


builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Create routes

app.MapPost("/carrinhos", async (Carrinho carrinho, IDistributedCache redis) =>
{
    await redis.SetStringAsync(carrinho.UsuarioId, JsonSerializer.Serialize(carrinho));
    return true;
});

app.MapGet("/carrinhos/{usuarioId}", async (string usuarioId, IDistributedCache redis) =>
{
    var data = await redis.GetStringAsync(usuarioId);

    if (string.IsNullOrWhiteSpace(data)) return null;

    var carrinho = JsonSerializer.Deserialize<Carrinho>(data, options: new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = false
    });

    return carrinho;
});
#endregion
app.Run();

record Carrinho (string UsuarioId, List<Produto> Produtos);
record Produto (string Nome, string Quantidade, decimal PrecoUnitario);
