# --- Étape 1 : Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Optimisation du cache : on restaure les dépendances avant de copier le code
COPY ["tadidyVeApi.csproj", "./"]
RUN dotnet restore "./tadidyVeApi.csproj"

# Copier tout le reste et builder
COPY . .
RUN dotnet publish "tadidyVeApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Étape 2 : Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Sécurité : On utilise l'utilisateur non-root par défaut de .NET 8
USER $APP_UID

# On laisse l'hébergeur (Render/Railway) injecter le port via la variable PORT
# Mais on garde 5223 comme valeur par défaut via la variable standard de .NET
ENV ASPNETCORE_HTTP_PORTS=5223

EXPOSE 5223

ENTRYPOINT ["dotnet", "tadidyVeApi.dll"]