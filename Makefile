commit:
	git add .
	git commit

tool:
	rm -rf ./dist ./bin ./obj
	dotnet pack -c Release -o ./dist
	dotnet tool uninstall nebula-stream -g || true
	dotnet tool install nebula-stream --global --add-source ./dist --prerelease

.PHONY: commit tool