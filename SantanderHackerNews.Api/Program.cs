using Polly;
using Polly.Extensions.Http;
using Santander.HackerNews.Api.Configuration;
using Santander.HackerNews.Api.Services;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Configuration.GetSection("HackerNews").Bind(Settings.HackerNews);

builder.Services.AddHttpClient("hackernews", client =>
{
    client.BaseAddress = new Uri(Settings.HackerNews.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSwaggerGen(c =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference(opt => opt
    .WithTitle("Santander Hacker News Api")
    .WithTheme(ScalarTheme.DeepSpace)
    .WithDefaultOpenAllTags(true)
    .WithDarkMode(true));

    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swaggerDoc, HttpReq) =>
            swaggerDoc.Servers.Add(
                new Microsoft.OpenApi.Models.OpenApiServer
                {
                    Url = $"https://{HttpReq.Host.Value}{app.Configuration.GetValue<string>("UrlPath")}",
                    Description = "Santander Hacker News Api"
                }));
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"{app.Configuration.GetValue<string>("UrlPath")}/swagger/v1/swagger.json", "Santander Hacker New V1");
        c.EnableTryItOutByDefault();
        c.DocExpansion(DocExpansion.None);
        c.DefaultModelRendering(ModelRendering.Example);
    });
}

app.UseRouting();
app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => (int)msg.StatusCode == 429) // too many requests
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
