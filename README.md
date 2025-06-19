Guardian is a .NET API that monitor for folders and automatically upload the new files to a drive service provider (Current setup use Google Drive API).

For my purpose it was running on Linux Debian system and expects to have "motion" package istalled as well as a DB server for metadata.

Motion is a program that monitors the signal from video cameras and detects changes in the images. (https://github.com/Motion-Project/motion)

Once motion create new video files the api is triggered and check until the recording as stopped until it eventually upload the file on the cloud.

The app can be manually triggered via the exposed apis that take care of the whole process.
