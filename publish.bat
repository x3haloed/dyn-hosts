dotnet publish -c Release -r win10-x64 /p:PublishSingleFile=true --self-contained true .\src\DynHosts.Client\DynHosts.Client.csproj
dotnet publish -c Release -r win10-x86 /p:PublishSingleFile=true --self-contained true .\src\DynHosts.Client\DynHosts.Client.csproj
dotnet publish -c Release -r win10-x64 --no-self-contained .\src\DynHosts.Server\DynHosts.Server.csproj
