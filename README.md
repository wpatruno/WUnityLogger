# WPLogger

## Introduction
This project is a better logging tool for Unity.
_I am sure other logging project can be better but I had fun making my own._

This project has two main feature : 
- Remove logs in build for better performances
- Tag to show/hide any logs.

Secondary features :
- Save logs to file
- Enable/Disable log to unity console (for performance)
- Enable/Disable log history (for performance)
- Event on log/error
- Option to Keep log in build
- Debug UI Prefab to display log in scene (Usefull when in build/devBuild)
- Show time / tags in log

## Log stripping
When building for release, Debug.Log are not usefull but stay in the codebase. The most common solution is to remove them at compilation. that's why I use the conditionnal attribute, available in C#, to keep logs only when conditions are met.

The 3 conditions to keep logs in code are :
- Unity Editor
- Development build
- "WPLOG" exist as a preprocessor directives

Error logs are never stripped.

## Tags
Tags are usefull when the project get bigger and contain lot of differents modules and tools.
Each tool and modules should add a tag unique to them when logging, this allow to only display logs that are currently needed for working.
A tag can be any string, but they are case sensitives and only some default tags are enabled.

The special tag "F" allow to force display the log even if his tags are disabled.

```
void SetTagActive(string tag)   // Allow a new tag to be logged

void SetTagDisabled(string tag) // Disable a tag from any new log

bool IsTagActive(string tag)    // Check if the tag is active
```

Error logs are not affected by tags but you can use tag to make them look better.

## Log
The function Log used to display log with the tag system
Trigger "OnLogged" event.
```
//Definition
void Log(string text, params string[] tags)

//Example
string varMyTag = "HelloTag"
WPLogger.Log("Hello !", varMyTag, "OtherTag");
```


## FLog
Fast&Force log doesn't implement the tag, events, history, time functions.
It only log to Unity console for better performance.
This function is still stripped from build.

## Error Log
The function Error is used to display errors with the tag system
Errors are not stripped from build and are always displayed even when all its tags are disabled.
Trigger "OnErrorLogged" event.

```
//Definition
void Error(string text, params string[] tags)

//Example
string varMyTag = "HelloTag"
WPLogger.Error("Error !", varMyTag, "OtherTag");
```

## Debug UI
A prefab to display Wplogger logs in scenes.
It allow easier understanding of logs and errors in builds

F12 to toggle display