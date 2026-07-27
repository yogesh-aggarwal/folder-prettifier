# Folder Prettifier

Your folders are a mess and it's keeping you up at night. We get it.

Folder Prettifier sweeps through any folder, neatly sorts every file into the right spot (Videos go to Videos, PDFs go to Documents, you get the idea), and gives everything a nice clean name. All in one click.

Perfect for that Downloads folder you've been ignoring for three years.

## What it does

- **Sorts everything** — throws every file into the right folder automatically
- **Cleans up names** — fixes capitalization, swaps out ugly words, adds prefixes or suffixes
- **Saves your skin** — makes a backup before touching anything, just in case you change your mind
- **Keeps itself updated** — grabs the latest sorting rules from the internet when it starts, but works fine offline too
- **Lives on a USB stick** — one file, nothing else, run it from anywhere
- **Right-click magic** — right-click any folder and pick **Folder Prettifier** (if you used the installer)

## What you need

- Windows 7 or newer
- That's pretty much it

## Getting started

1. Open the app (or right-click a folder → **Folder Prettifier**)
2. Pick the dumpster fire you want to clean up
3. Tell it what to do:
   - Turn **Categorize files** on or off
   - Turn **Prettify names** on and tweak the settings (caps lock, anyone?)
   - Give the whole folder a new name if you're feeling fancy
4. Hit **Process** — stand back and watch the magic

## Building from source

You'll need **Visual Studio 2022 Build Tools** (or full VS) with the **.NET desktop build workload**.

```sh
make          # builds everything
make bump 2.1.0   # bump version across all files
```

Or manually:

```sh
powershell -File scripts\build.ps1
```

The project also ships with a **GitHub Actions release pipeline** — tag a commit with `v*` and it builds, packages, and publishes the release automatically.

## Still have questions?

Yell at us at [https://yogeshaggarwal.in](https://yogeshaggarwal.in)

## License

MIT License — see [LICENSE](LICENSE)

Copyright © 2026 Yogesh Aggarwal

## Privacy

See [PRIVACY.md](PRIVACY.md)
