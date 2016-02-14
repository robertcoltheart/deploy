# Deploy
A simple framework for authoring MSI installation packages.

## Usage
```csharp
var builder = new PackageBuilder()
	.Author("Me")
	.Platform(PackagePlatform.X86)
	.ProductName("My Product")
	.UpgradeCode(Guid.NewGuid())
	.File("start.exe", "icon.ico", "Start Shortcut")
	.File("dependency.dll")
	.Version(new Version(1, 0, 0));
	
builder.Build("Setup.msi");
```

## Limitations
The package builder is designed to create very simple packages, and thus is limited by the following:

1. **No compression.** Files are packaged as a simple CAB and are not compressed.
2. **No control over icons.** Icons are placed in the root of the start menu only.
3. **No directory control.** All binaries are placed in the "Program Files/[ProductName]" directory. For 32 bit applications on 64-bit systems, the "Program Files (x86)" folder is used.
4. **No custom actions.** For extra actions such as bootstrapping or database setup, a fully-fledged setup suite should be used.
5. **No custom UI.** The MSI will install without prompts and will only show a progress bar.
