# PWManager

A simple and secure password manager using AES encryption with PBKDF2 key derivation.

## Overview

PWManager is a Windows desktop application developed in .NET 9.0 that allows you to store and manage your passwords securely. Data is encrypted before being stored in a local SQLite database, ensuring that your sensitive information remains protected.

## Features

- Secure password storage with AES encryption
- Intuitive graphical interface using Windows Forms
- Quick credential search
- Strong password generation
- Quick copy of passwords to clipboard
- Local storage of data in SQLite database
- Tracking of creation date and last update of passwords

## System Requirements

- Windows 10 or higher
- .NET 9.0 Runtime

## Technologies Used

- .NET 9.0
- Entity Framework Core 9.0.0
- SQLite
- Windows Forms
- Clean Architecture

## Project Structure

The solution follows the principles of Clean Architecture, divided into layers:

- **PWManager.Domain**: Contains domain entities and business rules
- **PWManager.Application**: Implements application use cases
- **PWManager.Infra**: Handles data persistence and external services
- **PWManager**: Main project with the graphical interface (Windows Forms)

## Installation

1. Download the repository to your local machine
2. Navigate to the project folder
3. Compile the solution using Visual Studio 2022 or command line:

```bash
dotnet build
```

## Execution

Run the compiled application or use the command:

```bash
cd src/PWManager
dotnet run
```

On first execution, you will be prompted to create a master password that will be used to encrypt and decrypt your stored passwords.

## Data Storage

By default, the SQLite database is stored in the user's AppData folder:

```
%APPDATA%\PWManager\pwmanager.db
```

The location and file name can be customized in the `appsettings.json` file.

## Security

- All passwords are encrypted using AES-256
- The master password is never stored, only used to derive the encryption key
- Uses PBKDF2 with sufficient iterations to make brute force attacks difficult

## Contributions

Contributions are welcome! Feel free to open issues or submit pull requests with improvements.

## License

This project is distributed under the MIT license.
