# FileMan

### Config
to change basic usage of the app 
```json
{
  "BackgroundColor":"White",
  "ForegroundColor":"Black",
  "ErrorLogColor":"Red",
  "LogFile": "app.log",
  "ShowHiddenByDefault": false,
  "FolderIcons" : "ShowWithPadding",
  "FileIcons" : "ShowWithPadding",
  "DisplayTrimOptions" : "TrimEnd",
  "DisplayItemSize" : false
}
```
Colors options: https://learn.microsoft.com/en-us/dotnet/api/system.consolecolor?view=net-10.0

FolderIconsOptions and FileIconsOptions:    Hide, Show, ShowWithPadding

Display Trim Options:     TrimStart, TrimEnd

Also allows a mod key to be set using [Enum From System](https://learn.microsoft.com/en-us/dotnet/api/system.consolemodifiers) but this is not officially supported by default mod key will be CTRL

### Controls
```text
Simple key bindings

left/right = move between tabs
up/down = move up and down in a tab (use mod key to jump 10 at a time)

(mod) + left/right = will create new tabs or collapse all tabs to right
pageup/pagedown = go to top of current tab or bottom
Enter = select

B = Top directory
K = will kill current tab
H = show hidden files on tab
(mod) + q = exit

F2 = Rename selected file or folder
F7 = New directory
F8 = New file
(mod) + d = Delete current selected file/folder

S = stores current selected item in buffer
(mod) + s = clears buffer
C = Copy (Copy the current item in buffer here)
M = move (Moves the current item in buffer here)
R = Reload current display
Q = Calculate Size toggle (sizes load async in background)
(mod) + l = Edit current path (Tab/Shift+Tab to cycle completions)
```
(mod) by default is control

![ImageOne](https://raw.githubusercontent.com/mkbmain/FileMangerCLI/main/Pics/Start.jpg)


## Simple Usage

![Image2](https://raw.githubusercontent.com/mkbmain/FileMangerCLI/main/Pics/1.jpg)
Highlighted above is the path you are currently in

![Image3](https://raw.githubusercontent.com/mkbmain/FileMangerCLI/main/Pics/2.jpg)
You can store data in the buffer using S key 

from here you can copy and move it to other folders
(it will go in to what ever tab you are currently in)

![Image4](https://raw.githubusercontent.com/mkbmain/FileMangerCLI/main/Pics/3.jpg)
You can open multiple tabs using mod+right


![Image5](https://raw.githubusercontent.com/mkbmain/FileMangerCLI/main/Pics/4.jpg)

as you can see as many as you like 
