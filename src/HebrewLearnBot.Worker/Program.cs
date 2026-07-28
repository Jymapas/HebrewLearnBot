// Заглушка каркаса (этап Э0). Заменяется полноценной точкой входа на этапе Э5b.
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("ok"));

await app.RunAsync();
