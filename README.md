# WPLogger

## Introduction

**Well's Personal Logger** is a better logging system for Unity in C#.

_I am sure other logging project can be better but I had fun making my own._

This project has two main features : 
- **Remove logs in build for better performances**
- **Tag to show/hide any logs.**

Secondary features :
- Save logs to file
- Enable/Disable log to unity console (for performance)
- Enable/Disable log history (for performance)
- Enable/Disable display time or tags in log
- Event on log and error
- Option to keep logs in build
- Debug UI Prefab to display logs in scene (Usefull when in build)


## Log stripping
When building for release, **Debug.Log are not usefull but stay in the codebase**. A better way of handling this is to **remove them at compilation**.

The tool use the conditionnal attribute, available in C#, to keep logs only when conditions are met.

The 3 conditions to keep logs in code are :
- Unity Editor
- Development build
- "WPLOG" exist as a preprocessor directives (Use editor window)

Error logs are never removed.

## Editor Window
Multiple options are available on the unity editor window on the top bar menu **Tools -> WPLogger**

Using the editor will ask you to select a folder to place required data files.

If you want to active the WPLOG preprocessor directive using the editor tool, it will create a csc.rsp file in Assets folder.

## Tags
Tags are usefull when the project get bigger and contain **lot of differents modules and tools**.

Each tool and modules should add a **tag unique** to them when logging, this allow to only display logs that are currently needed for working.

A tag can be any string, but they are case sensitives and only some default tags are enabled.

The special tag "F" allow to force display the log even if his tags are disabled.

Example:
```
WPLogger.Log("My message", WPTags.TESTTag, "anotherTag");
```

Managing active tags at runtime:

```
void SetTagActive(string tag)   // Allow a new tag to be logged

void SetTagDisabled(string tag) // Disable a tag from any new log

bool IsTagActive(string tag)    // Check if the tag is active
```

Error logs are not affected by tags but you can use tag for better clarity.

## Log
The function Log used to display log with the tag system

Trigger "OnLogged" event.
```
//Definition
void Log(string message, params string[] tags)

//Example
string varMyTag = "TEST"
WPLogger.Log(message);
WPLogger.Log(message, tag1, tag2);
WPLogger.Log("My message concerning AI", "AI", "NAVMESH", "ENNEMIES", varMyTag);
```


## FLog
Fast&Force log doesn't implement the tag, events, history, time functions.

It only log to Unity console for better performance.

This function is still stripped from build.

```
//Definition
void Flog(string message)

//Example
WPLogger.Flog("Log message that can't use tags");
```

## LogError
The function Error is used to display errors with the tag system

Errors are not stripped from build and are always displayed even when all tags are disabled.

Trigger "OnErrorLogged" event.

```
//Definition
void Error(string message, params string[] tags)

//Example
string varMyTag = "HelloTag"
WPLogger.Error("Error !", varMyTag, "OtherTag");
```

## Debug UI
A prefab to display Wplogger logs in scenes.
It allow easier understanding of logs and errors in builds

F12 to toggle display
