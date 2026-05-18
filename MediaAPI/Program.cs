using Amazon.S3;
using Amazon.S3.Model;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// carrega a configuração da AWS do arquivo appsettings.json para o ambiente de execução da aplicação
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
// registra o serviço do Amazon S3 para ser injetado em outros componentes da aplicação para trabalhar com buckets e objetos
builder.Services.AddAWSService<IAmazonS3>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapPost("upload", async (IFormFile file, IAmazonS3 s3Client) =>
{
    if(!file.ContentType.StartsWith("image/"))
    {
        return Results.BadRequest("Somente arquivos de imagem são permitidos.");
    }

    var request = new PutObjectRequest()
    {
        BucketName = "guilherme-image-storage",
        Key = "images/" + file.FileName,
        InputStream = file.OpenReadStream()
    };

    request.Metadata.Add("Content-Type", file.ContentType);
    await s3Client.PutObjectAsync(request);
    return Results.Accepted($"Imagem {file.FileName} enviada para o S3 com sucesso!");
}).DisableAntiforgery(); 

app.UseHttpsRedirection();

app.Run();