# Unreleased

# 4.26.0

### Infrastructure
* Some dependencies were updated
* You have to delete all db functions in masstransit sql transport database in order for its migrations to work.
* Improve open telemetry support:
  * Use otel-collector
  * Remove Jaeger/Prometheus exporters
  * Add docker compose and configurations to run:
    * otel-collector
    * jaeger
    * prometheus
* Don't deploy after failed build in BuildAndDeploy.ps1 file
* Some dependencies were updated v2
* Use central package versioning

### Navigator
* Add order mode to the settings with the ability to list new entries first
* Fix crash on non-webp images that have webp extension
* Pause preview video when opening full screen preview
* Remember video timing when switching between preview and fullscreen preview
* Fix source deletion in the collection settings
* Improve splashscreen
* Migrate project to CommunityToolkit.Mvvm
* New tool: quick tagging, CTRL+Q to open
* Show splashscreen longer until the app is fully loaded

### Arachne
* Add availability check service that will stop consumers when it's search engine isn't available

### Room
* Create activity for each PeriodicJob
* Improve rapid run scenario
* Remove periodic jobs, use file system watcher instead to track changes in source folders
* Optimize checking for existing files in one-folder scenario (1 db query instead of 1 per file)
* Fix crashes in FileSystemWatcherEventStream

### Lilin
* Optimize db queries for saving new parsed tags

# 4.25.0

### Arachne
* Ignore old and expired LoadTagHistory and LoadNoteHistory commands from mass transit

### Navigator
* Fix webp previews in the list view
* Add button to enable/disable imouto pics upload in Room

### Viewer
* Add jfif to the list of supported image formats

### Room
* Add in-memory settings to enable/disable imouto pics upload and api for it
* Add tests for the imouto pics upload settings

# 4.24.0

### Navigator
* Move extensions to common project
* Add settings for auto shuffle on every reload (default: false)
* Improve startup performance
* Clean up warnings and enable TreatWarningsAsErrors
* Support WebP format in FullScreenPreview
* Add splashscreen
* Show notes in full screen preview
* Fix duplicates in TagsEditVM when clicking right mouse button on image tag

### Harpy
* Add ability to download faved danbooru pictures through gelbooru
* Ignore banned posts without exceptions

### Infrastructure
* Simplify nswag generated clients
* Add incremental source generator for generating Add*WebApiClients methods
* Auto sign all commits and tags
* Remove RabbitMq and migrate to the new MassTransit SQL Transport

### Viewer
* Fix infinite loading
* Fix set association for image file types

### Room
* Remove quartz from Room and use simple PeriodicRunnerHostedService

# 4.23.0

### Navigator
* Add a command to revert selected items to the previous version,
so you can fix you mistake if you accidentally miss clicked
* Add pixel size to the file status window for images and ugoira
* Now you can scroll images with mouse wheel even when mouse cursor is not on the image itself
* Add popular character tags in tags editor
* Add selected files count to the status bar
* Change title bar to show file name in full screen preview mode
* Add time/length for videos in full screen preview mode
* Ignore unhandled exception in App.cs
* Refactor code style in several common converter and behaviors

### Arachne
* Update packages

### Lilin
* Fix error in searching tags by contains query string
* Add integration tests

### Infrastructure
* Move ImoutoRebirth.Hasami to its own folder

# 4.22.0

### Infrastructure
* Add hotkey descriptions for Viewer and Navigator desktop apps
* Upgrade to net 8.0

### Navigator
* Add an option to create tag as counter
* When adding tag to file automatically set value to Counter:1 
if tag is counter

### Lilin
* Add Options field to the tag entity to store counter flag

### Room
* Refactor entire service to DDD style architecture

# 4.20.0

### Infrastructure
* Optimize release notes 

### Navigator
* Add tag "my wallpapers" to the file when setting wallpaper 
from context menu
* Add counter tags initial support: you have to create tag 
with value "Counter:0",then you can increase it in left tags window
* Fixed bug with deletion of user tags with custom value

# 4.19.3

### Room
* Add cancellation support for room controllers
* Add separate query for filter exising file hashes
* Add memory cache for this filter

### Lilin
* Add memory cache for RelativesBatchQuery

### ImoutoExtensions
* Remove oboe and use simple fetch with AbortController

### Infrastructure
* Add changelog file
* Add build step to create release notes body based on latest changelog
