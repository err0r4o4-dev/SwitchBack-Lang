# SwitchBack Privacy Notice

SwitchBack performs keyboard-layout conversion entirely on the user's Windows
device. It has no server component, telemetry, analytics, advertising, or user
account system.

When the global hotkey is pressed, SwitchBack temporarily reads the text copied
from the current selection into process memory, converts it, and places the
result on the Windows Clipboard for pasting. It does not intentionally write
selected text to disk and this MVP does not keep conversion history.

SwitchBack attempts to restore the previous Clipboard contents after pasting.
Clipboard restoration is best-effort because other applications and Clipboard
managers can modify or observe the Clipboard at the same time. Users should not
use the conversion hotkey on passwords, recovery codes, private keys, or other
highly sensitive text.

Settings are stored locally at:

`%LOCALAPPDATA%\SwitchBack\settings.json`

If conversion history is added in a future version, it must remain opt-in,
local-only, clearable by the user, and disabled by default.
