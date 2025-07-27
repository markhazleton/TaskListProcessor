/**
 * TaskListProcessor Web - Streaming Module
 * Handles real-time data streaming and SignalR connections
 */

const Streaming = {
    connections: {},
    
    init() {
        this.setupStreamingUI();
        console.log('Streaming module initialized');
    },

    // Setup streaming UI elements
    setupStreamingUI() {
        // Find streaming containers
        const streamingContainers = document.querySelectorAll('[data-streaming]');
        
        streamingContainers.forEach(container => {
            const streamType = container.dataset.streaming;
            this.initializeStreamingContainer(container, streamType);
        });
    },

    // Initialize a streaming container
    initializeStreamingContainer(container, streamType) {
        // Add streaming indicator
        const indicator = document.createElement('div');
        indicator.className = 'streaming-indicator position-absolute top-0 end-0 m-2';
        indicator.innerHTML = `
            <span class="badge bg-success d-none" data-streaming-status="connected">
                <i class="bi bi-wifi"></i> Connected
            </span>
            <span class="badge bg-warning d-none" data-streaming-status="connecting">
                <i class="bi bi-arrow-clockwise animate-spin"></i> Connecting
            </span>
            <span class="badge bg-danger d-none" data-streaming-status="disconnected">
                <i class="bi bi-wifi-off"></i> Disconnected
            </span>
        `;
        
        container.style.position = 'relative';
        container.appendChild(indicator);
        
        // Set initial status
        this.updateStreamingStatus(container, 'disconnected');
    },

    // Update streaming status
    updateStreamingStatus(container, status) {
        const indicators = container.querySelectorAll('[data-streaming-status]');
        indicators.forEach(indicator => {
            indicator.classList.add('d-none');
        });
        
        const activeIndicator = container.querySelector(`[data-streaming-status="${status}"]`);
        if (activeIndicator) {
            activeIndicator.classList.remove('d-none');
        }
    },

    // Start streaming connection (placeholder implementation)
    startStreaming(containerId, endpoint) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container ${containerId} not found`);
            return;
        }
        
        this.updateStreamingStatus(container, 'connecting');
        
        // Simulate connection
        setTimeout(() => {
            this.updateStreamingStatus(container, 'connected');
            this.simulateDataStream(container);
            
            // Store connection reference
            this.connections[containerId] = {
                endpoint: endpoint,
                status: 'connected',
                container: container
            };
            
            window.TaskListProcessor.showNotification(
                `Streaming connection established for ${containerId}`, 
                'success'
            );
        }, 2000);
    },

    // Stop streaming connection
    stopStreaming(containerId) {
        const connection = this.connections[containerId];
        if (!connection) {
            console.warn(`No active connection for ${containerId}`);
            return;
        }
        
        this.updateStreamingStatus(connection.container, 'disconnected');
        delete this.connections[containerId];
        
        window.TaskListProcessor.showNotification(
            `Streaming connection closed for ${containerId}`, 
            'info'
        );
    },

    // Simulate data streaming (replace with actual SignalR implementation)
    simulateDataStream(container) {
        const streamData = () => {
            if (!this.connections[container.id]) return;
            
            // Generate random data point
            const dataPoint = {
                timestamp: new Date().toISOString(),
                value: Math.random() * 100,
                type: 'data-update'
            };
            
            // Dispatch custom event
            container.dispatchEvent(new CustomEvent('streaming-data', {
                detail: dataPoint
            }));
            
            // Continue streaming
            setTimeout(streamData, 1000 + Math.random() * 2000);
        };
        
        streamData();
    },

    // Handle streaming data
    handleStreamingData(containerId, callback) {
        const container = document.getElementById(containerId);
        if (!container) return;
        
        container.addEventListener('streaming-data', (event) => {
            callback(event.detail);
        });
    },

    // Create real-time log viewer
    createLogViewer(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;
        
        const logViewer = document.createElement('div');
        logViewer.className = 'log-viewer bg-dark text-light p-3 rounded';
        logViewer.style.cssText = `
            height: 300px;
            overflow-y: auto;
            font-family: 'Courier New', monospace;
            font-size: 0.875rem;
            line-height: 1.2;
        `;
        
        container.appendChild(logViewer);
        
        // Handle streaming log data
        this.handleStreamingData(containerId, (data) => {
            this.addLogEntry(logViewer, data);
        });
        
        return logViewer;
    },

    // Add log entry to viewer
    addLogEntry(logViewer, data) {
        const entry = document.createElement('div');
        entry.className = 'log-entry mb-1';
        
        const timestamp = new Date().toLocaleTimeString();
        const level = this.getLogLevel(data);
        const message = typeof data === 'string' ? data : JSON.stringify(data);
        
        entry.innerHTML = `
            <span class="text-muted">[${timestamp}]</span>
            <span class="badge bg-${level} me-2">${level.toUpperCase()}</span>
            <span>${message}</span>
        `;
        
        logViewer.appendChild(entry);
        
        // Auto-scroll to bottom
        logViewer.scrollTop = logViewer.scrollHeight;
        
        // Limit number of entries (keep last 100)
        const entries = logViewer.querySelectorAll('.log-entry');
        if (entries.length > 100) {
            entries[0].remove();
        }
    },

    // Get log level based on data
    getLogLevel(data) {
        if (typeof data === 'object' && data.level) {
            return data.level.toLowerCase();
        }
        
        // Simple heuristic based on content
        const message = typeof data === 'string' ? data : JSON.stringify(data);
        if (message.includes('error') || message.includes('Error')) return 'danger';
        if (message.includes('warn') || message.includes('Warning')) return 'warning';
        if (message.includes('info') || message.includes('Info')) return 'info';
        return 'secondary';
    },

    // Create real-time metrics display
    createMetricsDisplay(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;
        
        const metricsDisplay = document.createElement('div');
        metricsDisplay.className = 'metrics-display row g-3';
        
        const metrics = ['CPU', 'Memory', 'Network', 'Disk'];
        metrics.forEach(metric => {
            const col = document.createElement('div');
            col.className = 'col-md-3';
            col.innerHTML = `
                <div class="card text-center">
                    <div class="card-body">
                        <h5 class="card-title">${metric}</h5>
                        <div class="metric-value h2 text-primary" data-metric="${metric.toLowerCase()}">
                            0%
                        </div>
                        <div class="progress mt-2">
                            <div class="progress-bar" role="progressbar" 
                                 data-metric-bar="${metric.toLowerCase()}" 
                                 style="width: 0%"></div>
                        </div>
                    </div>
                </div>
            `;
            metricsDisplay.appendChild(col);
        });
        
        container.appendChild(metricsDisplay);
        
        // Handle streaming metrics data
        this.handleStreamingData(containerId, (data) => {
            this.updateMetrics(metricsDisplay, data);
        });
        
        return metricsDisplay;
    },

    // Update metrics display
    updateMetrics(metricsDisplay, data) {
        if (typeof data !== 'object' || !data.metrics) return;
        
        Object.entries(data.metrics).forEach(([metric, value]) => {
            const valueElement = metricsDisplay.querySelector(`[data-metric="${metric}"]`);
            const barElement = metricsDisplay.querySelector(`[data-metric-bar="${metric}"]`);
            
            if (valueElement && barElement) {
                valueElement.textContent = `${Math.round(value)}%`;
                barElement.style.width = `${Math.min(100, Math.max(0, value))}%`;
                
                // Update color based on value
                barElement.className = 'progress-bar';
                if (value > 80) barElement.classList.add('bg-danger');
                else if (value > 60) barElement.classList.add('bg-warning');
                else barElement.classList.add('bg-success');
            }
        });
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    Streaming.init();
});

// Expose to global namespace
window.TaskListProcessor.Streaming = Streaming;

export default Streaming;
