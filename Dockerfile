# 应用镜像（ADR-0022）：多阶段构建，运行阶段用 chiseled-extra（无 shell、非 root、带 ICU 与时区数据）。
# 一份文件两个目标：api（Api / Worker 角色，同一镜像，按 Kmic__Roles 区分）和 migrator（迁移程序，部署时先于应用运行）
#   docker build --target api -t kmic-api .
#   docker build --target migrator -t kmic-migrator .
# 配置、连接串、密钥都不进镜像，由部署时的环境变量与挂载文件提供

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Hosts/Kmic.Api -c Release -o /out/api \
 && dotnet publish src/Hosts/Kmic.MigrationRunner -c Release -o /out/migrator

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS api
WORKDIR /app
COPY --from=build /out/api .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Kmic.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS migrator
WORKDIR /app
COPY --from=build /out/migrator .
ENTRYPOINT ["dotnet", "Kmic.MigrationRunner.dll"]
