# Unreleased

# 4.20.0

### Infrastructure
* Optimize release notes 

### Navigator
* Add tag "my wallpapers" to the file when setting wallpaper from context menu
* Add counter tags initial support: you have to create tag with value "Counter:0",
then you can increase it in left tags window
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
