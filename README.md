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
- `Algorithms`: Contains compiled `.dll` files for each metaheuristic algorithm module.
- `Functions`: Includes compiled `.dll` files for different fitness function modules.
- `Controllers`: Manages the request handling for the API endpoints.
- `Interfaces`: Defines contracts for services and algorithm behaviors.
- `Properties`: Configuration settings for the project.
- `Services`: Core business logic for managing algorithms and fitness functions.
- `Util`: Utility classes and helper functions.
- `Program.cs`: The entry point of the application.

## API Endpoints

Below is a table of the main API endpoints available in the system:

| Method | Endpoint                | Description                                       |
|--------|-------------------------|---------------------------------------------------|
| GET    | `/api/algorithms`       | Retrieves all available algorithms.               |
| POST   | `/api/algorithms/run`   | Runs a selected algorithm with specified settings.|
| POST   | `/api/algorithms/add`   | Adds a new algorithm to the system.               |
| PUT    | `/api/algorithms/update/{name}` | Updates an existing algorithm.             |
| DELETE | `/api/algorithms/delete/{name}` | Deletes an existing algorithm. 

## Requirements

To run the project, you need:
- .NET 7.0 environment installation.
- (Optionally) IDE such as Visual Studio or Visual Studio Code for easier editing and debugging.

## Installation and Running

1. Clone the repository or download the project files.
2. Open the project in your chosen IDE.
3. Install the required dependencies using NuGet.
4. Build and run the project.
5. Access to the API is possible through a browser or API client (e.g., Postman).


## License

The project is available under the MIT license.

