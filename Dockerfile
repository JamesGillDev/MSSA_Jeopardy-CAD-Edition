FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MSSA Jeopardy/MSSA Jeopardy.csproj", "MSSA Jeopardy/"]
COPY ["Shared/JeopardyGameService.Shared.cs", "Shared/"]
RUN dotnet restore "MSSA Jeopardy/MSSA Jeopardy.csproj"

COPY . .
WORKDIR "/src/MSSA Jeopardy"
RUN dotnet publish "MSSA Jeopardy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MSSA_Jeopardy.dll"]
