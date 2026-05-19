# mpv-protocol-handler

Windows handler for Emby/Xiaoya `mpv://play/<base64url>` links.

The web page generates a URL like:

```text
mpv://play/aHR0cDovLzE5Mi4xNjguMS4xMDE6...
```

`mpv.net` cannot consume that custom protocol directly. This small executable receives the `mpv://` URL from Windows, decodes the base64url payload into the real media URL, then opens it with `mpv.net` or `mpv`.

## Install

### Current user, no administrator required

Copy `dist\mpv-protocol-handler.exe` to:

```text
%LOCALAPPDATA%\mpv-protocol-handler\mpv-protocol-handler.exe
```

Then import:

```text
registry\register-current-user.reg
```

You can also run:

```powershell
.\scripts\install-current-user.ps1
```

### All users, administrator required

Copy `dist\mpv-protocol-handler.exe` to:

```text
C:\Program Files\mpv-protocol-handler\mpv-protocol-handler.exe
```

Then import:

```text
registry\register-all-users.reg
```

You can also run an elevated PowerShell:

```powershell
.\scripts\install-all-users.ps1
```

After installation, click the `MPV` button in Emby/Xiaoya.

The browser prompt should mention `mpv-protocol-handler.exe`, not Windows PowerShell.

## Player lookup order

The handler looks for a player in this order:

1. Environment variable `MPV_PROTOCOL_PLAYER`
2. `mpvnet.exe` or `mpv.exe` in the same folder as the handler
3. `C:\Program Files\mpv.net\mpvnet.exe`
4. `C:\Program Files (x86)\mpv.net\mpvnet.exe`
5. `C:\Program Files\mpv\mpv.exe`
6. `C:\Program Files (x86)\mpv\mpv.exe`
7. `mpvnet.exe` or `mpv.exe` on `PATH`

## Build

On Windows, use the .NET Framework compiler:

```powershell
New-Item -ItemType Directory -Path dist -Force | Out-Null
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /optimize+ /out:dist\mpv-protocol-handler.exe src\MpvProtocolHandler.cs
```

## Uninstall

Import the matching removal file:

- Current user: `registry\remove-current-user.reg`
- All users, administrator required: `registry\remove-all-users.reg`

Then delete:

```text
C:\Program Files\mpv-protocol-handler
%LOCALAPPDATA%\mpv-protocol-handler
```
