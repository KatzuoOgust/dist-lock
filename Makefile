.DEFAULT_GOAL := help

.PHONY: help build test pack clean format

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) \
		| awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-10s\033[0m %s\n", $$1, $$2}'

build: ## Build the solution
	dotnet build DistLock.slnx

test: ## Run all tests
	dotnet test DistLock.slnx --logger "console;verbosity=normal"

pack: ## Pack NuGet packages to ./artifacts/nupkgs
	dotnet pack DistLock.slnx -c Release --output ./artifacts/nupkgs

clean: ## Remove build artefacts (bin/obj/artifacts)
	dotnet clean DistLock.slnx
	rm -rf artifacts

format: ## Format source with dotnet format
	dotnet format DistLock.slnx
