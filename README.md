# TaskListProcessor
[Article On This Repo](https://markhazleton.controlorigins.com/task-list-processor.html)

## Overview
The `TaskListProcessor` class is a .NET utility for efficient asynchronous task management. It enables concurrent execution, result tracking, and detailed telemetry, ideal for applications that need to handle multiple tasks simultaneously.

## Features
- **Concurrent Task Execution**: Run multiple tasks in parallel.
- **Result Tracking**: Capture and store the results of tasks.
- **Performance Telemetry**: Generate detailed telemetry for task performance analysis.
- **Error Logging**: Robust error handling and logging capabilities.

## Why I Wrote the Task List Processor

The inception of the Task List Processor was fueled by the desire to address a common yet under-discussed challenge in modern .NET development: efficiently managing multiple asynchronous operations that may have varying return types and distinct completion times. My journey in the .NET landscape has shown me that while asynchronous programming is powerful, it can also introduce complexity that necessitates a thoughtful approach to error handling, performance measurement, and resource management.

In working on projects that required fetching and processing data from various external services concurrently—such as building a travel site dashboard with real-time weather information for multiple cities—I realized the need for a tool that could handle these operations in a streamlined and error-resilient manner. This led to the creation of the Task List Processor, a class that embodies the principles of concurrent task management and robust logging.

Another motivation was to share knowledge and solutions with the community. The Task List Processor is not just a utility; it's an embodiment of best practices and design patterns in asynchronous .NET programming. It serves as an educational tool for other developers to learn from and build upon.

Lastly, I am a firm believer in lifelong learning and continuous improvement. Developing this project has been an avenue for me to explore new ideas, refine my skills, and contribute to the collective wisdom of the developer ecosystem. By open-sourcing the Task List Processor, I aim to invite collaboration, feedback, and innovation, keeping in stride with the ever-evolving field of technology.

## Usage

```csharp
var taskListProcessor = new TaskListProcessor();
var tasks = new List<Task>
{
    // Add your tasks here
};
await TaskListProcessor.WhenAllWithLoggingAsync(tasks, logger);
```


## Getting Started

The Task List Processor project is designed to be simple to set up and run, even if you're new to .NET development. Below are the instructions to get you started with this project:

### Prerequisites

Before you begin, ensure you have the following installed:
- [Git](https://git-scm.com/downloads)
- [.NET Latest SDK](https://dotnet.microsoft.com/download)

### Installation

1. **Clone the repository**

Use the following Git command to clone the project repository to your local machine:

```sh
   git clone https://github.com/markhazleton/TaskListProcessor.git
```

2.  **Navigate to the project directory**
    
    Once cloned, move into the project directory with:
    
    ```sh
    cd TaskListProcessor
    ```
    
3.  **Build the project**
    
    To build the project and restore any dependencies, run the following command:
    
    ```sh
    dotnet build
    ```
    
    This will compile the project and prepare it for running.
    

### Running the Application

After building the project, you can run the console application using the .NET CLI:

```sh
dotnet run --project ./examples/TaskListProcessor.Example/TaskListProcessor.Example.csproj
```

### What to Expect

Once the application is running, it will:

-   Simulate fetching weather data for a predefined list of cities.
-   Log the progress and results of each operation to the console.
-   Demonstrate handling of asynchronous tasks, errors, and telemetry.

By following these instructions, you should have the Task List Processor up and running on your local machine, ready to be explored and expanded upon.


## Contact
- email:mark.hazleton@controlorigins.com 
- https://www.linkedin.com/in/mark-hazleton/
- https://markhazleton.controlorigins.com 

## License

The Task List Processor is open-sourced under the MIT License. This license permits anyone to freely use, modify, distribute, and sublicense the code, provided that they include the original copyright notice and license disclaimer in any copy or significant portion of the software.

Here's what you can do with the Task List Processor under the MIT License:

- **Use** the code for commercial purposes, personal projects, or educational purposes.
- **Modify** the code to your personal preference to fit into your project.
- **Distribute** the original or modified version of the code.
- **Sublicense** it if you need to include it in a larger project with more restrictive distribution terms.

The MIT License is one of the most permissive and open licenses available, promoting open-source collaboration and the free exchange of knowledge and technology. It is a good choice for this project because it allows for the greatest freedom in use and distribution, aligning with my goal of supporting the developer community.

For the full license text, please see the [LICENSE](LICENSE) file in the repository.
