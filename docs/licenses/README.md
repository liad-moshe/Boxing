# Licenses

## Our obligation
Godot is released under the **MIT license**. MIT lets us use, modify and sell the game freely, on one
condition: every copy we distribute must include Godot's **copyright notice and license text**. Godot
also bundles third-party components (for example FreeType, ENet and mbedTLS) that have their own
notices, and these must be credited too. Exported C# builds also ship .NET runtime files, which are
MIT-licensed by the .NET Foundation and need the same treatment.

Official guidance: https://docs.godotengine.org/en/4.7/about/complying_with_licenses.html

Because the game uses no external assets, there are no art, font or sound licenses to track. Any NuGet
package that ships inside the game build (not test-only packages) must be added here with its license.

## Backlog item: in-game Licenses screen (M1)
The M1 main menu has a **Licenses** entry (docs/roadmap.md). The screen shows text that comes from the
engine at runtime, so it is always correct for the exact engine build:
- `Engine.GetLicenseText()`: Godot's own MIT license text.
- `Engine.GetCopyrightInfo()`: copyright entries for Godot and each bundled third-party component.
- `Engine.GetLicenseInfo()`: the full text of each license those components use.

The screen also shows a short .NET runtime notice. The exact wording is decided when the screen is
specified.
