<h1 align="center">JsonCrypter</h1>

<div align="center">
  <strong>Encrypts and decrypts the values of JSON files while preserving their structure.</strong><br>
  AES-GCM 256 authenticated encryption, with per-value keys derived from a password via Argon2id.<br>
  <sub>Tampered data or a wrong password are detected and rejected.</sub>
</div>

<br>

<div align="center">
  <!-- .NET -->
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge" alt=".NET 10">
  <!-- License -->
  <img src="https://img.shields.io/badge/license-MIT-blue?style=for-the-badge" alt="License: MIT">
</div>

## Table of Contents

- [Features](#features)
- [How it works](#how-it-works)
- [Prerequisites](#prerequisites)
- [Usage](#usage)
- [Build from source](#build-from-source)
- [Running a downloaded release](#running-a-downloaded-release)
- [Publishing a standalone executable](#publishing-a-standalone-executable)
- [License](#license)

## Features

- Encrypts every value of a JSON file while preserving its structure (keys, nesting, and arrays stay intact).
- Decrypts files previously produced by the app.
- Authenticated encryption (AES-GCM): tampered data or a wrong password are detected and rejected.
- Per-value key derivation with Argon2id and a random salt and nonce for each value.

## How it works

Each leaf value is processed independently:

1. A random 16-byte **salt** is generated, and a 256-bit key is derived from the password with **Argon2id**.
2. The value is encrypted with **AES-GCM** using a random 12-byte **nonce**, producing the ciphertext and a 16-byte authentication **tag**.
3. The output value is the Base64 string of `salt | nonce | tag | ciphertext`.

Decryption reverses the process: it reads the salt and nonce back from the payload, re-derives the key, and verifies the authentication tag before returning the plaintext.

> **Note:** values are stored as text, so non-string JSON values (numbers, booleans) come back as strings after a round-trip (for example, `30` becomes `"30"`).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) - required to build, run, or publish from source.
- To run a **published standalone executable**, nothing else is needed: the .NET runtime is bundled.

## Usage

```bash
JsonCrypter -o <operation> -f <file> -p <password>
```

| Option | Alias | Required | Description |
|--------|-------|----------|-------------|
| `--operation` | `-o` | Yes | The operation to perform: `encrypt` or `decrypt`. |
| `--file` | `-f` | Yes | Path to the input file. Must be a `.json` file (the extension check is case-insensitive). |
| `--password` | `-p` | Yes | The password used to derive the key. Use a strong, hard-to-guess value. |

> The file is processed **in place**: the input file is overwritten with the result.

A sample input file is available at [resources/example.json](resources/example.json):

```json
{
  "glossary": {
    "title": "example glossary",
    "GlossDiv": {
      "title": "S",
      "GlossList": {
        "GlossEntry": {
          "ID": "SGML",
          "SortAs": "SGML",
          "GlossTerm": "Standard Generalized Markup Language",
          "Acronym": "SGML",
          "Abbrev": "ISO 8879:1986",
          "GlossDef": {
            "para": "A meta-markup language, used to create markup languages such as DocBook.",
            "GlossSeeAlso": [
              "GML",
              "XML"
            ]
          },
          "GlossSee": "markup"
        }
      }
    }
  }
}
```

Encrypt it:

```bash
JsonCrypter -o encrypt -f example.json -p mysecretpassword
```

```json
{
  "glossary": {
    "title": "wb14PYz5YGSQsTCB1/oPNBD3o6vdm79/MH/BW+zmzpBTaDidnxejQGVnGeRyRZMMsWWy9uBQUf8N0+do",
    "GlossDiv": {
      "title": "lErvS47Zbo3UacP+hP7odNg/uimyDN6JxCCwB7rf9XmCi7SQgN6hE0/39a4Q",
      "GlossList": {
        "GlossEntry": {
          "ID": "RbfJEp4rX+cKuHfXstgobKLrhk8jgJM/WHkbWQxMB16qHm+JwOim0iOotzJoKtA8",
          "SortAs": "td0lwe7PNiRd19eeypQNOorrjNUgSfl0jyu0or50bMqgfBSjMCbNtBGYAxT9Q9rT",
          "GlossTerm": "6ZCwxmJc+RKF6MLvpQhgnD+VKGYwkK18jG72QBJ3WA+Vb3iXugmetEsbFx8tqfamrEAHn1ao6FhnOhuZpbtj71lP7o4G+q+BK192KEBdI9A=",
          "Acronym": "tkbx1SbN4mj2/czFOiJzR3yOwQ82VBkUS3nSv3LK8TwvUUaOd+6y9LYEz41rr5pq",
          "Abbrev": "wPwt5EQc0wtCbaqyrc36/GqIMC1NuD7Iumfyr8hluwb7MS6JPNiCxaPGJXoCQUoZQYIw2kiC+OmK",
          "GlossDef": {
            "para": "ZajqoGaeSTrWAn7/BzbaTnJPVIAFXPEefqIkntD45kw7TViyQ1aHIwI/LeDk2XdquWnQCZozr3h5QPtdbet/XGaHN4sPPhnlmnjrEzENUxxIV5VsR5z7bRQ8H+JJPdPjBLMPz/D0rDoW4oXvNXLQqzcs3Zc=",
            "GlossSeeAlso": [
              "YKSXnlD9yqa02yi642igIZsgA2msIVLZicFWI2lkicv9o4VQcx9mWVqc2Mn3Cuk=",
              "YGQwfGea6anui/I3OjTltRfuG08BmwV2P32ExMarI/cAUceCcyvLuhG0BXVSDOE="
            ]
          },
          "GlossSee": "I7JE852kqoaqQryWINSyRBCuoKjOFGHJReNUrEBufaNjN5pPcfWXm0hD9YAgZjP+t28="
        }
      }
    }
  }
}
```

> The encrypted output is randomized: a fresh random salt and nonce are used for every value, so each run produces different ciphertext. The values above are just one illustrative example - yours will differ, and they all decrypt back to the same original content.

Decrypt it again with the same password to restore the original content:

```bash
JsonCrypter -o decrypt -f example.json -p mysecretpassword
```

## Build from source

```bash
git clone https://github.com/fabriziobagala/json-crypter.git
cd json-crypter
dotnet build
```

To run directly with the SDK:

```bash
dotnet run --project src/JsonCrypter -- -o encrypt -f example.json -p mysecretpassword
```

## Running a downloaded release

After downloading and extracting the archive for your platform, the executables are **not code-signed**, so each operating system needs a small one-time step to allow them to run.

### macOS

```bash
curl -L -o JsonCrypter.zip https://github.com/fabriziobagala/json-crypter/releases/download/v1.0.0/JsonCrypter-v1.0.0-osx-arm64.zip
unzip JsonCrypter.zip
chmod +x JsonCrypter                          # ensure it is executable
xattr -d com.apple.quarantine JsonCrypter     # clear the Gatekeeper quarantine flag
./JsonCrypter -o encrypt -f example.json -p mysecretpassword
```

Alternatively, run it once, then go to **System Settings → Privacy & Security** and click **Open Anyway**.

### Linux

```bash
curl -L -o JsonCrypter.zip https://github.com/fabriziobagala/json-crypter/releases/download/v1.0.0/JsonCrypter-v1.0.0-linux-x64.zip
unzip JsonCrypter.zip
chmod +x JsonCrypter
./JsonCrypter -o encrypt -f example.json -p mysecretpassword
```

### Windows

```powershell
Invoke-WebRequest -Uri https://github.com/fabriziobagala/json-crypter/releases/download/v1.0.0/JsonCrypter-v1.0.0-win-x64.zip -OutFile JsonCrypter.zip
Expand-Archive JsonCrypter.zip -DestinationPath .
.\JsonCrypter.exe -o encrypt -f example.json -p mysecretpassword
```

If SmartScreen shows *"Windows protected your PC"*, click **More info → Run anyway**.

## Publishing a standalone executable

You can package JsonCrypter as a **self-contained, single-file executable** that runs on a machine without the .NET runtime installed.

```bash
dotnet publish src/JsonCrypter/JsonCrypter.csproj \
  -c Release \
  -r <RID> \
  --self-contained \
  -p:PublishSingleFile=true
```

Replace `<RID>` with the [runtime identifier](https://learn.microsoft.com/dotnet/core/rid-catalog) for your target platform:

| Platform | RID |
|----------|-----|
| Windows x64 | `win-x64` |
| Windows ARM64 | `win-arm64` |
| Linux x64 | `linux-x64` |
| Linux ARM64 | `linux-arm64` |
| macOS x64 (Intel) | `osx-x64` |
| macOS ARM64 (Apple Silicon) | `osx-arm64` |

The resulting executable is written to `src/JsonCrypter/bin/Release/net10.0/<RID>/publish/`. With `--self-contained` the SDK produces a native host automatically, so the single file is everything you need to ship.

Example (macOS Apple Silicon):

```bash
dotnet publish src/JsonCrypter/JsonCrypter.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
./src/JsonCrypter/bin/Release/net10.0/osx-arm64/publish/JsonCrypter -o encrypt -f example.json -p mysecretpassword
```

To reduce the size of the bundled file, you can enable compression:

```bash
-p:EnableCompressionInSingleFile=true
```

> **Trimming and Native AOT are not recommended.** The command-line parsing relies on runtime reflection, which `PublishTrimmed=true` or `PublishAot=true` can break. Stick to the self-contained single-file build above for a reliable standalone executable.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
