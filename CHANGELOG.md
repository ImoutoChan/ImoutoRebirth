# Unreleased

### Navigator
* Add a command to revert selected items to the previous version,~~~~
so you can fix you mistake if you accidentally miss clicked
* Add pixel size to the file status window for images and ugoira
* Now you can scroll images with mouse wheel even when mouse cursor is not on the image itself
* Add popular character tags in tags editor
* Add selected files count to the status bar

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
