PLUGIN_NAME = Watchbook
BUILD_DIR = bin/Release/net8.0/publish

# Local Jellyfin config folder
LOCAL_JELLYFIN_CONFIG = ~/jellyfin/data/plugins/$(PLUGIN_NAME)

# ----------------------------
# Default: build and deploy
# ----------------------------
deploy: build copy

# Build the plugin
build:
	dotnet publish -c Release

# Copy only DLL and dependencies (skip config.json)
copy:
	mkdir -p $(LOCAL_JELLYFIN_CONFIG)
	cp $(BUILD_DIR)/Watchbook.dll $(LOCAL_JELLYFIN_CONFIG)

# Clean local build artifacts
clean:
	dotnet clean
	rm -rf $(BUILD_DIR)
