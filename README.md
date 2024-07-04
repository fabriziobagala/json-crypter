# JsonCrypter

A console app for encrypting and decrypting JSON files using AES-GCM 256, with keys generated via Argon2id.

## Features

- Encrypt JSON files with AES-GCM 256
- Decrypt JSON files encrypted with this application
- Generate secure keys using Argon2id

## Prerequisites

Before you start, make sure you have met the following requirement:

- [.NET SDK](https://dotnet.microsoft.com/en-us/download)

## Usage

To use the app, you have the following options:

```bash
./JsonCrypter [-o <operation>] [-f <file>] [-p <password>]
```

- `-o, --operation`: The operation to perform. This can be either `encrypt` or `decrypt`.
- `-f, --file`: The path to the input file. This should be a JSON file for encryption, or a file encrypted by this application for decryption.
- `-p, --password`: The password to use for encryption or decryption. This should be a secure, hard-to-guess password.

### Examples

Here are some examples of how you can use JsonCrypter:

1. To encrypt a file named `example.json` with the password `mysecretpassword`:

    ```bash
    ./JsonCrypter -o encrypt -f example.json -p mysecretpassword
    ```

    ```json
    {
        "glossary": {
            "title": "6HJXh57UZCyTnE/XaI91/AAAAAAAAAAAAAAAAF+b9F1+RyaUBWYppkYHva4VgkI05gwxgLreSdx2SY1T",
            "GlossDiv": {
                "title": "fkqOChGUupyTRrnLtIc8HQAAAAAAAAAAAAAAALU7Sc+HvZj3TjFTNXoxLm1e",
                "GlossList": {
                    "GlossEntry": {
                        "ID": "7oCV5I1RfacrzOoXxFJcTAAAAAAAAAAAAAAAAF++xpALQ+G/YrpwPKqQMw1EBMCQ",
                        "SortAs": "oufbmMQv0/2fIotMEPovagAAAAAAAAAAAAAAAEXyogJNwz3RQrVxiSabHPqxgA5S",
                        "GlossTerm": "ilmzBl6bys+NVuqbuqtrywAAAAAAAAAAAAAAANrHSNBNteNyXYG3aJX2KM11i/6/dUcELcnBdFyF5kDZ0p7Ixl18aqroqFCSYYC2VXX+COQ=",
                        "Acronym": "qFK+FlennKamA9paWwjyqgAAAAAAAAAAAAAAADIgpwUitWyz/sEbs8+Ah0C+IeUI",
                        "Abbrev": "mu7ulNgmqrIY1Pm3aBeHhgAAAAAAAAAAAAAAAAJA8QdQSdjPa6d6lL94bMJlAIEj9o/CP9KgHcb5",
                        "GlossDef": {
                            "para": "BUngNo9sD4Tc869wGQ0gOAAAAAAAAAAAAAAAABHgDd2n+E/M3Llt1sIrzfDx5t4rO1uRPZY1OaQiYmcV5SY2loxj4NZq1JyxzzpmN6aHi6/sNCSaSgVvkpib4UjiZ/AkOCs9v4NxQmQYE8cqoLtFe8k2ifQ=",
                            "GlossSeeAlso": [
                                "4yBI9i5subSJ1Oln0D/LAQAAAAAAAAAAAAAAABsJi4cG/wt2UvuB74qWl9xFkNk=",
                                "ZD/VOscqGkKrReYsX8OYXwAAAAAAAAAAAAAAAHWKnlVHT4/wBTxnBOUZCVztRgU="
                            ]
                        },
                        "GlossSee": "oKGpQ3RZR5E2RXHmrA4fWAAAAAAAAAAAAAAAANqPfodruVIswyrtiXsmlp5ljx2MziM="
                    }
                }
            }
        }
    }
    ```

2. To decrypt a file named `example.json` with the password `mysecretpassword`:

    ```bash
    ./JsonCrypter -o decrypt -f example.json -p mysecretpassword
    ```

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

## Publish

### AppHost

First, you need to instruct the SDK to generate a native executable for your application. To do this, add the `UseAppHost` property to your `.csproj` file within a `PropertyGroup`. Your `.csproj` file should look something like this:

```xml
<PropertyGroup>
  ...
  <UseAppHost>true</UseAppHost>
</PropertyGroup>
```

This configuration ensures that your application uses a native host, which is particularly beneficial when you're targeting specific runtimes or when you want your application to be self-contained.

Once you have configured your project file, you can use the `dotnet publish` command to package your application. The command for publishing should specify the configuration, runtime, and publish options. Here's how you can structure the publish command:

```bash
dotnet publish -c Release -r <RUNTIME_IDENTIFIER> --self-contained -p:PublishSingleFile=true -p:UseAppHost=true
```

Replace `<RUNTIME_IDENTIFIER>` with the appropriate runtime identifier for your target platform (e.g., `win-x64`, `linux-x64`, etc.). This command will generate a single file for your application, tailored for the specified runtime, and it will include the native host as specified by the `UseAppHost` setting.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
