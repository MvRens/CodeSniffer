FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
WORKDIR /srv
COPY . .
COPY ../../../../frontend .
ENTRYPOINT ["dotnet", "CodeSniffer.dll"]