# MacroDetect v1.0

## Logger (`macrodetect-v1.0.exe`)
This is the very first release of my attempt to make a macro detector.
Running the **.exe** will open a console window and start a keylogger (not malware I promise :sob:).

This will log all inputs + the delay in between inputs in milliseconds/microseconds to a **.csv** file in `~/Downloads`.

> You can close the console window by selecting it and pressing `ctrl+c`.

## Analyzer (`analyzer.html`)

Open the `.html` file in your browser and drop the generated `.csv` into the analyzer, enter the suspect's hotbar keybinds for each item, and click **Analyze**. It will show:

- **Average Timings** — mean time for each sequence (obbi->crystal, anchor->glowstone->totem and airplace), filtering out segments over 500ms
- **Sequence Details** — each recorded sequence with deltas
- **All Tracked Inputs** — full table of only the configured item keys + clicks, with proper deltas (it will only show the selected keybinds, no movement keys etc.)
> If you want to view all keybinds, not just the selected ones, they are in the generated **.csv** file

Works with regular keyboard keys and mouse side buttons (`x1click`, `x2click`).


## Compile from source

If you don't trust the **.exe** (which you shouldn't :smiley_cat:) you can always easily compile `Program.cs` from source:

First, clone the repository:
```bash
git clone https://github.com/oogaboogasisser/macrodetect.git
cd ./macrodetect
```
Then compile:
```powershell
# requires the .NET SDK
dotnet build -c Release
```

The output is in `bin/Release/net8.0/macrodetect.exe`.

