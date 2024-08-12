# LingoShift

LingoShift is a powerful and efficient translation application designed to seamlessly integrate into your workflow. It provides quick translation capabilities with hotkey support, allowing users to translate text between languages (primarily English and Italian) with ease.

## Features

- Translate text between languages (primarily English and Italian)
- Hotkey support for quick translations
- Clipboard integration for easy text input and output
- Popup display of translation results
- Cross-platform support (Windows focus)
- Configurable settings for API keys and translation sequences
- Persistent storage of settings using SQLite

## Project Structure

The project follows a clean architecture approach, divided into several layers:

1. **LingoShift.Domain**: Contains the core business logic and entities.
2. **LingoShift.Application**: Implements application services and defines DTOs and interfaces.
3. **LingoShift.Infrastructure**: Provides implementations for external services, platform-specific functionalities, and data persistence.
4. **LingoShift.Desktop**: The main entry point for the desktop application.
5. **LingoShift**: Contains the user interface components and viewmodels.

## How to Use

### Default Hotkeys

LingoShift provides several default hotkeys for quick translations:

1. **Ctrl + Shift + E**: Translate selected text to English and replace in the clipboard.
2. **Ctrl + Shift + I**: Translate selected text to Italian and replace in the clipboard.
3. **Ctrl + P, I**: Translate selected text to English and show in a popup.
4. **Ctrl + P, E**: Translate selected text to Italian and show in a popup.

### Translation Process

1. Select the text you want to translate.
2. Use one of the hotkey combinations mentioned above.
3. The translated text will either replace the original text in your clipboard or appear in a popup, depending on the hotkey used.

### Configuring Settings

1. Open the LingoShift application.
2. Click on the "Settings" button in the top-right corner of the main window.
3. In the Settings window, you can:
   - Enter your OpenAI API key
   - Add, remove, or modify translation sequences
4. Click "Save Settings" to persist your changes.

## Development

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or later (recommended)
- Entity Framework Core tools (dotnet-ef)

### Building the Project

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/LingoShift.git
   ```
2. Open the `LingoShift.sln` solution file in Visual Studio.
3. Build the solution (F6 or Build > Build Solution).

### Database Initialization

The database is automatically initialized and updated when the application starts. The `InitializeDatabase` method in the `Startup.cs` file handles this process using Entity Framework Core migrations.

### Adding New Tables

To add a new table to the database:

1. Create a new entity class in the `LingoShift.Domain/Entities` folder.
2. Add a corresponding `DbSet<YourNewEntity>` property in the `AppDbContext` class (`LingoShift.Infrastructure/Data/AppDbContext.cs`).
3. Open a terminal in the project directory and run the following commands:
   ```
   dotnet ef migrations add AddYourNewEntityTable -p LingoShift.Infrastructure -s LingoShift.Desktop
   ```
4. The new migration will be created in the `LingoShift.Infrastructure/Migrations` folder.
5. The next time you run the application, the new table will be automatically created in the database.

### Running the Application

1. Set `LingoShift.Desktop` as the startup project.
2. Run the application (F5 or Debug > Start Debugging).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Google Translate API for providing translation services.
- OpenAI API for advanced language processing capabilities.
- Avalonia UI framework for cross-platform UI development.
- Entity Framework Core and SQLite for data persistence.

---

For more information or support, please open an issue in the GitHub repository.