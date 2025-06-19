using Guardian.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors(app.Configuration.GetSection("Cors:Policy").Get<string>() ?? "");
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();