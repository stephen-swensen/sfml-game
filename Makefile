run:
	dotnet run --project src/sfml.fsproj --tailcalls+ --debug-

build:
	dotnet build src/sfml.fsproj

format:
	dotnet fantomas src 



