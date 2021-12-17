var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseCors(builder => builder.WithOrigins("http://localhost:3000").AllowAnyHeader());

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MapControllerRoute(
//    name: "deneme",
//    pattern: "{MediaUrl0}/{MediaContentType0}/{SmsMessageSid}/{NumMedia}/{ ProfileName}/{ SmsSid}/{ WaId}/{ SmsStatus}/{ Body}/{ To}/{ NumSegments}/{ MessageSid}/{ AccountSid}/{ From}/{ ApiVersion}",
//    defaults: new { controller = "Home", action = "Send" });

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
