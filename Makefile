run:
	dotnet run --project src/sfml.fsproj --tailcalls+ --debug-

build:
	dotnet build src/sfml.fsproj

format:
	dotnet fantomas src 

publish:
	dotnet publish --output="./publish/linux-x64" --runtime linux-x64 --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
	cp -r assets publish/linux-x64

