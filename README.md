# mpv-protocol-handler

Windows handler for Emby/Xiaoya `mpv://play/<base64url>` links.

The web page generates a URL like:

```text
mpv://play/aHR0cDovLzE5Mi4xNjguMS4xMDE6...
```

`mpv.net` cannot consume that custom protocol directly. This small executable receives the `mpv://` URL from Windows, decodes the base64url payload into the real media URL, then opens it with `mpv.net` or `mpv`.

## Install

### Copy the handler

Copy `dist\mpv-protocol-handler.exe` to:

```text
C:\Program Files\mpv.net\mpv-protocol-handler.exe
```

This keeps the handler beside `mpvnet.exe`. Copying into `C:\Program Files` requires administrator permission.

### Current user protocol registration

Import:

```text
registry\register-current-user.reg
```

This registers only the current Windows user, but still expects the executable at:

```text
C:\Program Files\mpv.net\mpv-protocol-handler.exe
```

You can also run PowerShell as administrator:

```powershell
.\scripts\install-current-user.ps1
```

If UAC asks for a different administrator account, import `registry\register-current-user.reg` again after the copy step while signed in as the target user.

### All users protocol registration

Import as administrator:

```text
registry\register-all-users.reg
```

You can also run an elevated PowerShell:

```powershell
.\scripts\install-all-users.ps1
```

After installation, click the `MPV` button in Emby/Xiaoya.

The browser prompt should mention `mpv-protocol-handler.exe`, not Windows PowerShell.

If it still mentions Windows PowerShell, the current user probably still has an old `HKCU\Software\Classes\mpv` registration. Import `registry\register-current-user.reg` for that user, or remove the stale per-user key with `registry\remove-current-user.reg` so the all-users `HKLM` registration can take effect.

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
C:\Program Files\mpv.net\mpv-protocol-handler.exe
```
