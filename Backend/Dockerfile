# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Lưu ý: Thay "Backend.csproj" bằng tên file .csproj thực tế của bạn nếu khác
COPY ["Backend.csproj", "./"]
RUN dotnet restore "./Backend.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Giai đoạn chạy (Run)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
# Lưu ý: Thay "Backend.dll" bằng tên file DLL của bạn (thường trùng với tên Project)
ENTRYPOINT ["dotnet", "Backend.dll"]