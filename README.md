# Metaheuristic Algorithm Testing System

## Description

This project is a system for testing various metaheuristic algorithms on selected objective (fitness) functions. The system allows for the loading and management of different algorithms and fitness functions, parameterization, and the automatic selection of the best parameters for the tested algorithms.

## Features

- Load and manage metaheuristic algorithms.
- Load and manage fitness functions.
- Ability to conduct tests on selected algorithms and fitness functions.
- Automatic selection of the best parameters for algorithms.
- Saving and loading the state of experiments.
- Generating reports in PDF format.

## Technologies

The project is implemented in C# using:
- .NET 7.0
- ASP.NET Core for REST API
- iTextSharp for generating PDF reports

## Project Structure

The project consists of several main components:
- `API` - REST API interface allowing interaction with the system.
- `Core` - Business logic responsible for managing algorithms and experiments.
- `Data Access` - Data access abstraction layer.
- `Models` - Data model classes.
- `Services` - Services performing specific business logic tasks.

## Requirements

To run the project, you need:
- .NET 7.0 environment installation.
- (Optionally) IDE such as Visual Studio or Visual Studio Code for easier editing and debugging.

## Installation and Running

1. Clone the repository or download the project files.
2. Open the project in your chosen IDE.
3. Install the required dependencies using NuGet.
4. Configure the database in the `appsettings.json` configuration file.
5. Build and run the project.
6. Access to the API is possible through a browser or API client (e.g., Postman).


## License

The project is available under the MIT license.

