#pwd=Ngaq
#docker build -t ngaq-server:1.0 -f ./Ngaq.Server/Dockerfile .
#docker run -p 2341:2341 --name ngaq-server ngaq-server:1.0
#構建上下文在Ngaq/ 洏不在Ngaq/Ngaq.Server
# 构建阶段（使用.NET 9 SDK）
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
# 复制解决方案文件（优化层缓存）
#COPY ["Ngaq.sln", "."]
#COPY ["src/*/*.csproj", "src/"]
COPY ["Ngaq.Core/Ngaq.Core.csproj", "Ngaq.Core/Ngaq.Core.csproj"]
COPY ["Ngaq.Server/proj/Directory.Build.props", "Ngaq.Server/proj/Directory.Build.props"]
COPY ["Ngaq.Server/proj/Directory.Packages.props", "Ngaq.Server/proj/Directory.Packages.props"]
COPY ["Ngaq.Server/proj/Ngaq.Biz/Ngaq.Biz.csproj", "Ngaq.Server/proj/Ngaq.Biz/Ngaq.Biz.csproj"]
COPY ["Ngaq.Server/proj/Ngaq.Web/Ngaq.Web.csproj", "Ngaq.Server/proj/Ngaq.Web/Ngaq.Web.csproj"]

#RUN dotnet new sln -n TeQuaero.Backend
#RUN dotnet sln add src/**/*.csproj
# 还原NuGet包
# 先複製.sln和.csproj文件進行dotnet restore，利用Docker層緩存加速構建 。
# 避免一次性複製所有文件，防止宿主機路徑結構汙染容器。
#RUN dotnet restore "TeQuaero.Backend.sln"
#RUN cd src/TeQuaero.Web/ && dotnet restore TeQuaero.Web.csproj && cd ../..
#RUN cd ./Ngaq.Server/proj/Ngaq.Web/ && dotnet restore
# 复制所有源码
COPY . .

# 发布项目
RUN dotnet publish "Ngaq.Server/proj/Ngaq.Web/Ngaq.Web.csproj" -c Release -o /app/publish

# 运行阶段（使用ASP.NET运行时）
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
# 暴露端口（根据实际项目端口配置）
EXPOSE 2341
ENTRYPOINT ["dotnet", "Ngaq.Web.dll"]
