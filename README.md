# PWManager

A simple and secure password manager using AES-256 encryption with PBKDF2 key derivation.

## Overview

PWManager is a cross-platform desktop application developed in .NET 10.0 that allows you to store and manage your passwords securely. Data is encrypted before being stored in a local SQLite database, ensuring that your sensitive information remains protected.

The UI was migrated from Windows Forms to **Avalonia UI** with a modern dark theme. The security, encryption, and persistence logic remains unchanged.

## Features

- Secure password storage with AES-256 encryption
- Modern dark UI using Avalonia UI (MVVM pattern) — dark charcoal + teal accent color scheme
- Master password unlock screen with metallic button style
- Add, edit, delete password entries
- Global search bar in the header (filters entries by site or login simultaneously)
- Inline filter strip above the entries grid (separate site and login filters)
- Built-in password generator card positioned above the entries grid for a faster generate → fill → save flow
- Quick copy of passwords to clipboard
- Local storage in SQLite database (data never leaves your device)
- Tracking of creation date and last update of passwords

## UI Layout

```
┌─ Header: Logo | Storage status | Global search bar ─────────────┐
│ Sidebar │ [Add New Entry card]                                    │
│  (home) │ [Password Generator card]                              │
│         │  Filter by site…  |  Filter by login…  | Clear        │
│         │ [Entries DataGrid]                                      │
└─ Footer: Encrypted notice | Entry count ────────────────────────┘
```

## System Requirements

- Windows 10 or higher (or any OS supported by Avalonia)
- .NET 10.0 Runtime

## Running the Application

```bash
cd src/PWManager.Avalonia
dotnet run
```

Or build and run the release:

```bash
dotnet build PWManager.sln -c Release
dotnet src/PWManager.Avalonia/bin/Release/net10.0/PWManager.Avalonia.exe
```

## Technologies Used

- .NET 10.0
- Avalonia UI 12.x (cross-platform UI)
- CommunityToolkit.Mvvm (MVVM)
- Entity Framework Core 10.x
- SQLite (local encrypted storage)
- Microsoft.Extensions.DependencyInjection
- Clean Architecture (Domain / Application / Infra / UI)

## Security Notes

- Master password is **never stored in plain text**
- All password entries are encrypted with AES-256 before being saved
- Key derivation uses PBKDF2 (Rfc2898DeriveBytes)
- Passwords are only decrypted in-memory when displayed
- The SQLite database file is located at `%AppData%\PWManager\pwmanager.db`

## Architecture

```
PWManager.Domain       - Entities, repository contracts, service contracts
PWManager.Application  - Use cases (IUserApplication)
PWManager.Infra        - EF Core DbContext, repositories, encryption service
PWManager.Avalonia     - Avalonia UI (MVVM): Views, ViewModels, Services
PWManager (legacy)     - Original Windows Forms UI (kept for reference)
```

## Project Structure

The solution follows the principles of Clean Architecture, divided into layers:

- **PWManager.Domain**: Contains domain entities and business rules
- **PWManager.Application**: Implements application use cases
- **PWManager.Infra**: Handles data persistence and external services
- **PWManager.Avalonia**: Main project with the graphical interface (Avalonia UI)

## Color Scheme

The UI uses a dark charcoal + teal accent palette:

| Role | Value |
|---|---|
| App background | `#111118` |
| Card background | `#1C1C26` |
| Accent (teal) | `#3ECAB5` |
| Text primary | `#E8E8F2` |
| Text secondary | `#8888A8` |
