# TaskListProcessor Web Application

A comprehensive web demonstration of the TaskListProcessor library, showcasing advanced task orchestration, telemetry, and streaming capabilities with a modern Bootstrap 5 interface.

## 🚀 Quick Start

### Prerequisites

- .NET 10.0 SDK or later
- A modern web browser with JavaScript enabled

### Running the Application

1. **Build the solution:**

   ```bash
   dotnet build
   ```

2. **Run the web application:**

   ```bash
   dotnet run
   ```

3. **Open your browser and navigate to:**
   - `https://localhost:5001` (HTTPS)
   - `http://localhost:5000` (HTTP)

## 📋 Features

### 🏠 Home Page

- **Interactive Demo Panel** - Configure and run processing scenarios
- **Real-time Results** - Live updates with telemetry data
- **Hero Section** - Overview of capabilities and benefits
- **Feature Cards** - Highlighting key functionality

### 🎯 Demo Dashboard

- **Multiple Processing Scenarios:**
  - Main Processing - Batch processing with dependencies
  - Individual Task Processing - Single task execution
  - Cancellation Demo - Demonstrating graceful cancellation
  - Streaming Processing - Real-time streaming results
- **Live Progress Tracking** - Real-time progress bars and status updates
- **Interactive Configuration** - Customize processing parameters
- **Streaming Results** - Server-sent events for real-time updates

### 📖 Documentation

- **Complete API Reference** - All interfaces and classes
- **Implementation Examples** - Code samples and best practices
- **Architecture Overview** - System design and patterns
- **Migration Guide** - Upgrading from legacy versions
- **Interactive Table of Contents** - Easy navigation

### 📊 Performance Analysis

- **Benchmark Results** - Performance metrics and comparisons
- **Memory Usage Analysis** - Memory consumption patterns
- **Chart Visualizations** - Interactive charts with Chart.js
- **Optimization Tips** - Performance improvement recommendations
- **System Requirements** - Hardware and software requirements

### 🏗️ Architecture View

- **Visual Architecture Diagram** - System component overview
- **Design Principles** - SOLID principles implementation
- **Component Details** - Interface segregation and decorators
- **Code Examples** - Implementation patterns and usage

## 🛠️ Technology Stack

### Backend

- **ASP.NET Core 10.0** - Web framework
- **TaskListProcessor Library** - Core processing engine
- **Dependency Injection** - Built-in DI container
- **Structured Logging** - Microsoft.Extensions.Logging

### Frontend

- **Bootstrap 5** - Modern CSS framework
- **Bootstrap Icons** - Comprehensive icon library
- **Chart.js** - Interactive data visualization
- **Vanilla JavaScript** - Custom interactivity
- **Server-Sent Events** - Real-time streaming

### Services Integration

- **CityWeatherService** - Weather data simulation
- **CityThingsToDoService** - Activity data simulation
- **TaskProcessingService** - Main orchestration service

## 🎮 Usage Examples

### Basic Processing

```javascript
// Configure processing options
const config = {
    selectedCities: ['London', 'Paris', 'New York'],
    maxConcurrentTasks: 10,
    timeoutMinutes: 5,
    enableDetailedTelemetry: true,
    scenario: 'MainProcessing'
};

// Start processing
fetch('/Home/ProcessTasks', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(config)
});
```

### Streaming Processing

```javascript
// Start streaming connection
const eventSource = new EventSource('/Home/ProcessStreamingTasks');

eventSource.onmessage = function(event) {
    const result = JSON.parse(event.data);
    updateUI(result);
};

eventSource.onerror = function(event) {
    console.error('Streaming error:', event);
};
```

## 🔧 Configuration Options

### Processing Configuration

- **Selected Cities** - Choose cities for processing
- **Max Concurrent Tasks** - Control parallelism (1-20)
- **Timeout Minutes** - Task timeout duration (1-30)
- **Detailed Telemetry** - Enable comprehensive metrics
- **Individual Results** - Show per-task results
- **Processing Scenario** - Select execution pattern

### Telemetry Options

- **Performance Metrics** - Execution times and throughput
- **Memory Usage** - Memory consumption tracking
- **Error Tracking** - Comprehensive error analysis
- **Health Monitoring** - System health indicators

## 📁 Project Structure

```
TaskListProcessor.Web/
├── Controllers/
│   └── HomeController.cs          # Main MVC controller
├── Services/
│   └── TaskProcessingService.cs   # Core processing service
├── Models/
│   ├── ViewModels.cs              # View models for UI
│   └── ProcessingModels.cs        # Processing configuration models
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml           # Landing page
│   │   ├── Demo.cshtml            # Interactive demo
│   │   ├── Documentation.cshtml   # API documentation
│   │   ├── Performance.cshtml     # Performance analysis
│   │   └── Architecture.cshtml    # Architecture overview
│   ├── Shared/
│   │   ├── _Layout.cshtml         # Main layout
│   │   └── Error.cshtml           # Error page
│   └── _ViewImports.cshtml        # View imports
├── wwwroot/
│   ├── css/
│   │   └── site.css               # Custom styles
│   └── js/
│       └── site.js                # Custom JavaScript
├── Program.cs                     # Application entry point
└── TaskListProcessor.Web.csproj  # Project file
```

## 🚦 Processing Scenarios

### 1. Main Processing

- Demonstrates batch processing with city data
- Shows dependency resolution and parallel execution
- Includes comprehensive telemetry and error handling

### 2. Individual Task Processing

- Processes a single city's data
- Showcases task isolation and focused execution
- Ideal for debugging and focused testing

### 3. Cancellation Demo

- Demonstrates graceful task cancellation
- Shows proper cleanup and resource management
- Tests cancellation token propagation

### 4. Streaming Processing

- Real-time streaming of processing results
- Server-sent events for live updates
- Progressive result delivery and UI updates

## 🎨 UI Components

### Custom Components

- **Status Indicators** - Visual status representation
- **Progress Bars** - Animated progress tracking
- **Metric Cards** - Performance metric display
- **Loading Spinners** - Loading state indicators
- **Notification System** - Toast notifications

### Interactive Elements

- **Copy-to-Clipboard** - Easy code copying
- **Smooth Scrolling** - Enhanced navigation
- **Responsive Design** - Mobile-friendly layout
- **Tooltips** - Contextual help information

## 🔍 Monitoring & Debugging

### Telemetry Data

- **Execution Times** - Task duration tracking
- **Memory Usage** - Memory consumption analysis
- **Error Rates** - Success/failure statistics
- **Throughput Metrics** - Tasks per second

### Debug Features

- **Request IDs** - Unique request tracking
- **Detailed Logging** - Comprehensive log output
- **Error Pages** - User-friendly error handling
- **Browser Console** - JavaScript debugging

## 🚀 Deployment

### Development

```bash
dotnet run --environment Development
```

### Production

```bash
dotnet publish -c Release
dotnet TaskListProcessor.Web.dll
```

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "TaskListProcessor.Web.dll"]
```

## 🧪 Testing

### Manual Testing

1. **Navigate to the Demo page**
2. **Configure processing options**
3. **Run different scenarios**
4. **Monitor real-time results**
5. **Verify telemetry data**

### Performance Testing

1. **Access the Performance page**
2. **Review benchmark results**
3. **Analyze memory usage patterns**
4. **Check optimization recommendations**

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**
3. **Make your changes**
4. **Test thoroughly**
5. **Submit a pull request**

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:

- **Documentation** - Check the in-app documentation
- **Issues** - Create GitHub issues for bugs
- **Discussions** - Use GitHub discussions for questions

## 🔗 Related Projects

- **TaskListProcessor** - Core processing library
- **CityWeatherService** - Weather data service
- **CityThingsToDoService** - Activity data service

---

**Built with ❤️ using ASP.NET Core and TaskListProcessor**
