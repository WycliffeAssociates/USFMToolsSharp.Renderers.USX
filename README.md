# USFMToolsSharp.Renderers.USX

A USX (Unified Scripture XML) renderer for USFM (Unified Standard Format Markers) documents. This library converts USFM formatted scripture text into valid USX XML format, supporting both USX 2.5 and 3.0 specifications.

## Overview

USFMToolsSharp.Renderers.USX is a .NET library that provides rendering capabilities to convert USFM documents into USX XML format. USX is an XML-based scripture markup language used by digital Bible translation and publishing tools.

## Features

- Support for USX 2.5 and 3.0 specifications
- Configurable rendering options
- Partial USX output support (excluding XML declaration and root element)

## Installation

Install the package via NuGet:

```bash
dotnet add package USFMToolsSharp.Renderers.USX
```

Or using the NuGet Package Manager:

```
Install-Package USFMToolsSharp.Renderers.USX
```

## Usage

### Basic Example

```csharp
using USFMToolsSharp;
using USFMToolsSharp.Renderers.USX;

// Parse a USFM document
var parser = new USFMParser();
var usfmDocument = parser.ParseFromString(usfmText);

// Create a renderer with default configuration (USX 2.5)
var renderer = new USXRenderer();

// Render to USX
string usxOutput = renderer.Render(usfmDocument);
```

### Using USX 3.0

```csharp
using USFMToolsSharp;
using USFMToolsSharp.Renderers.USX;

// Parse USFM document
var parser = new USFMParser();
var usfmDocument = parser.ParseFromString(usfmText);

// Configure for USX 3.0
var config = new USXConfig
{
    USXVersion = "3.0",
    PartialUSX = false
};

var renderer = new USXRenderer(config);
string usxOutput = renderer.Render(usfmDocument);
```

### Partial USX Output

Use partial USX output when you need just the content without XML declaration and root `<usx>` element:

```csharp
using USFMToolsSharp;
using USFMToolsSharp.Renderers.USX;

var parser = new USFMParser();
var usfmDocument = parser.ParseFromString(usfmText);

// Configure for partial output
var config = new USXConfig
{
    USXVersion = "3.0",
    PartialUSX = true  // Excludes XML declaration and <usx> root element
};

var renderer = new USXRenderer(config);
string partialUsxOutput = renderer.Render(usfmDocument);
```

## Configuration

The `USXConfig` class provides the following configuration options:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `USXVersion` | `string` | `"2.5"` | The USX specification version to use. Supported values: `"2.5"`, `"3.0"` |
| `PartialUSX` | `bool` | `false` | When `true`, excludes the XML declaration (`<?xml...?>`) and root `<usx>` element from output |

## Building

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Build Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/WycliffeAssociates/USFMToolsSharp.Renderers.USX.git
   cd USFMToolsSharp.Renderers.USX
   ```

2. Restore dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```

3. Build in Release mode:
   ```bash
   dotnet build -c Release
   ```

4. Create a NuGet package:
   ```bash
   dotnet pack -c Release
   ```
   The package will be created in `USFMToolsSharp.Renderers.USX/bin/Release/`

## Dependencies

- [USFMToolsSharp](https://github.com/WycliffeAssociates/USFMToolsSharp)

## Related Projects

- [USFMToolsSharp](https://github.com/WycliffeAssociates/USFMToolsSharp) - The core USFM parsing library
- [USFM Documentation](https://ubsicap.github.io/usfm/) - Official USFM specification
- [USX Documentation](https://ubsicap.github.io/usx/) - Official USX specification

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## Authors

- @Thundelly
- @rbnswartz

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/WycliffeAssociates/USFMToolsSharp.Renderers.USX).
