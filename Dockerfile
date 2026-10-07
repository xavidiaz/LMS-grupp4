# Images for the Raspberry Pi demo, built by .github/workflows/publish.yml.
# Two targets: api (LMS.API) and web (LMS.Blazor, the BFF in front of it).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

# SQL Server has no image that runs on the Pi (arm64), so the API uses
# SQLite there: the provider is swapped here at build time and the schema is
# created from the model on startup (the SQL Server migrations don't apply to
# SQLite). The group's code stays as it is; the build stops if the lines
# patched here change.
FROM build AS build-api
RUN ef=$(grep -oP 'Include="Microsoft\.EntityFrameworkCore" Version="\K[^"]+' LMS.Infrastructure/LMS.Infrastructure.csproj) \
 && dotnet add LMS.API package Microsoft.EntityFrameworkCore.Sqlite --version "$ef" \
 && sed -i 's/options\.UseSqlServer(connectionString)/options.UseSqlite(connectionString)/' LMS.API/Program.cs \
 && sed -i 's/^\(\s*\)var app = builder\.Build();/&\n\1using (var scope = app.Services.CreateScope()) scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();/' LMS.API/Program.cs \
 && grep -q 'UseSqlite' LMS.API/Program.cs && grep -q 'EnsureCreated' LMS.API/Program.cs \
 && dotnet publish LMS.API -c Release -o /app

FROM build AS build-web
RUN dotnet publish LMS.Blazor/LMS.Blazor -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
LABEL org.opencontainers.image.source=https://github.com/xavidiaz/LMS-grupp4
WORKDIR /app
COPY --from=build-api /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "LMS.API.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS web
LABEL org.opencontainers.image.source=https://github.com/xavidiaz/LMS-grupp4
WORKDIR /app
COPY --from=build-web /app .
# Login sessions are saved in App_Data/ under the content root; the root
# filesystem is read-only on the Pi, so App_Data points at the /tmp tmpfs.
RUN ln -s /tmp /app/App_Data
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "LMS.Blazor.dll"]
