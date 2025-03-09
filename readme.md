# Windows Desktop Icon Position Backup

#### _Simple and small program to backup current position, size and view settings for the icons on the desktop, and load them whenever you want._

## Downlaod

_See the **release section**._

## Usage and installation

This program is a portable exe, so it doesnt need any installation.<br/>
You can backup the current desktop icons state with the add button _(after typing a backup name if wanted)_, and load one of the saved backups just by selecting it and clicking Apply.

## Backups save path

This program saves all the backups in an SQLite database, placed in the user's `Local` folder `C:\Users\{User}\AppData\Local\IconPosBackup\db.sqlite`.

## How does it work

The desktop icons state is located in the Desktop registry key: `HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\Shell\Bags\1\Desktop`, this program simply reads all the values in that key and stores them in the database.<br/>
When you select Apply the program closes the `Explorer` process, loads the selected backup back in the registry, and then restarts `Explorer`.