# Unreleased

### Highlights
* NEW FEATURE Integrity reports (Settings => Integrity Reports) will anylize your collections for missing or corrupted files
* Significantly improved video slider seek experience

### Infrastructure
* Migrated to .NET 10 runtime
* Updated all dependencies

### Navigator
* Various performance improvements
* NEW FEATURE Integrity reports (Settings => Integrity Reports) will anylize your collections for missing or corrupted files
* Supports continuations and export to the different formats (csv json)
* Significantly improved video slider seek experience
* Fixed video pause/play to trigger on click down instead of button release

# v4.29.1

### Highlights

* This version introduces new "create collection" window with multi-step wizard interface
* New service Lamia extracts media tags from files (codec, resolution, duration) and adds them to your files
* Pause/resume video on click in player area in Navigator

### Navigator
* Add close button for the quick tagging window
* Settings are now preserved between versions of Navigator apps
* Reformat added on date for files
* **New "create collection" with multi-step wizard interface**
* Ask for confirmation before deleting a collection in Collections view
* Pause/resume video on click in player area

### Lamia
* New service lamia, extracts media tags from files (codec, resolution, duration) and adds them to Lilin

### Room
* Add webhook upload settings to source folder configuration (enable/disable toggle and URL field)

### Infrastructure
* Add shared icon to all Windows service executables

# v4.28.2

### Highlights
* This version introduces the new install/update app
* You can call it with install.cmd, dependencies will be installed together with the app itself
* All important configurations are also available for edit from this ui now, you no longer need to edit json file
* Navigator: 
  * hot keys cheat sheet, 
  * cbr cbz archives (dodji) support
  * quick tagging improvements
* Viewer
  *  Support opening zipped galleries: zip 7z rar cbz cbr, convinient to view your dodji archives without extracting
* Auto tagging
  * Exhentai and schale are added for dodji tagging sources, please provide your credentials for them

### Infrastructure
* Migrate to .NET 9
* Update all dependencies
* Add Resqueue dashboard
* Add new videos in README for the initial launch and configuration of collections
* Fix various issues after migration to .NET 9
* New installer experience with Tori UI

### Navigator
* Add date in file info (left bottom corner)
* When you click calculate hash, it will be compared with the stored hash for the file; 
if they are different, the hash will be highlighted in red
* Support for dodji (archives or comic formats) and their previews
* Add hot keys cheat sheet in the bottom of tags view
* Add some basic animation for hot keys sheet sheet and quick tagging
* Add new tag pack set button for quick tagging
* Add reset last played positions button in settings
* Close all flyouts on escape
* Repeated T will close the tags edit flyout
* Escape to exit from flyouts
* Autofocus search tag field in tags edit
* Fix the bug in quick add tags when different sets have the same key
* Add ctrl shift space hotkey for quick tagging to select the previous tag pack set

### Viewer
* Support opening zipped galleries: zip 7z rar cbz cbr etc It will be extracted to a temporary folder and opened 
as a folder. You can set up ImoutoViewer as a default app to open cbz for example.

### Room
* File formats are only checked if the file has a supported image extension. So it means you are no longer required
to create 2 different source folder entries with and without this flag.

### Arachne
* ExHentai support as tags provider for files (.zip, .rar, .7z, .tar, .ace, .cbz, .cbr, .cb7, .cbt, .cba)
* Fill the login data in the config from cookies
* Improve ExHentai search and selection metadata capabilities
* Add gelbooru settings, since you can no longer use their api without an account

# 4.26.1

### Infrastructure
* Fix the issue with migrating masstransit schema, now it will be done automatically

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
