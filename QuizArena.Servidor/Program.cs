var builder = WebApplication.CreateBuilder(args);

// Registrar los controladores de nuestra API REST.
builder.Services.AddControllers();

var app = builder.Build();

// Asociar las rutas HTTP con los controladores.
app.MapControllers();

app.Run();
