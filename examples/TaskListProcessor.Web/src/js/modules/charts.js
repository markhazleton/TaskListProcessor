/**
 * TaskListProcessor Web - Charts Module
 * Handles chart rendering and data visualization
 */

const Charts = {
    charts: {},
    
    init() {
        this.initializeCharts();
        console.log('Charts module initialized');
    },

    // Initialize all charts on the page
    initializeCharts() {
        const chartContainers = document.querySelectorAll('[data-chart-type]');
        
        chartContainers.forEach(container => {
            const chartType = container.dataset.chartType;
            const chartId = container.id || `chart-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
            
            if (!container.id) {
                container.id = chartId;
            }
            
            this.createChart(chartId, chartType, container.dataset);
        });
    },

    // Create a chart based on type and configuration
    createChart(chartId, chartType, config) {
        // For now, create a placeholder chart
        // In a real implementation, you would integrate with Chart.js, D3.js, or another charting library
        const container = document.getElementById(chartId);
        if (!container) return;

        // Create a simple SVG chart placeholder
        const chart = this.createPlaceholderChart(chartType, config);
        container.innerHTML = chart;
        
        // Store chart reference
        this.charts[chartId] = {
            type: chartType,
            config: config,
            container: container
        };
    },

    // Create placeholder chart (replace with actual chart library integration)
    createPlaceholderChart(type, config) {
        const width = config.width || 400;
        const height = config.height || 300;
        
        let chartContent = '';
        
        switch (type) {
            case 'bar':
                chartContent = this.createBarChartPlaceholder(width, height);
                break;
            case 'line':
                chartContent = this.createLineChartPlaceholder(width, height);
                break;
            case 'pie':
                chartContent = this.createPieChartPlaceholder(width, height);
                break;
            case 'donut':
                chartContent = this.createDonutChartPlaceholder(width, height);
                break;
            default:
                chartContent = this.createDefaultChartPlaceholder(width, height);
        }
        
        return `
            <div class="chart-container">
                <svg width="${width}" height="${height}" class="chart-svg">
                    ${chartContent}
                </svg>
                <div class="chart-legend mt-2">
                    <small class="text-muted">Chart data will be loaded dynamically</small>
                </div>
            </div>
        `;
    },

    // Create bar chart placeholder
    createBarChartPlaceholder(width, height) {
        const bars = [];
        const barWidth = 40;
        const spacing = 20;
        const maxBarHeight = height - 60;
        
        for (let i = 0; i < 5; i++) {
            const barHeight = Math.random() * maxBarHeight + 20;
            const x = 50 + i * (barWidth + spacing);
            const y = height - barHeight - 30;
            
            bars.push(`
                <rect x="${x}" y="${y}" width="${barWidth}" height="${barHeight}" 
                      fill="var(--bs-primary)" opacity="0.8" rx="4">
                    <animate attributeName="height" from="0" to="${barHeight}" dur="1s" fill="freeze"/>
                    <animate attributeName="y" from="${height - 30}" to="${y}" dur="1s" fill="freeze"/>
                </rect>
            `);
        }
        
        return bars.join('');
    },

    // Create line chart placeholder
    createLineChartPlaceholder(width, height) {
        const points = [];
        const segments = 8;
        
        for (let i = 0; i <= segments; i++) {
            const x = 50 + (i * (width - 100) / segments);
            const y = 50 + Math.random() * (height - 100);
            points.push(`${x},${y}`);
        }
        
        return `
            <polyline fill="none" stroke="var(--bs-primary)" stroke-width="3" 
                     points="${points.join(' ')}" opacity="0.8">
                <animate attributeName="stroke-dasharray" from="0,1000" to="1000,0" dur="2s" fill="freeze"/>
            </polyline>
            ${points.map(point => {
                const [x, y] = point.split(',');
                return `<circle cx="${x}" cy="${y}" r="4" fill="var(--bs-primary)"/>`;
            }).join('')}
        `;
    },

    // Create pie chart placeholder
    createPieChartPlaceholder(width, height) {
        const centerX = width / 2;
        const centerY = height / 2;
        const radius = Math.min(width, height) / 2 - 20;
        
        const slices = [
            { percentage: 30, color: 'var(--bs-primary)' },
            { percentage: 25, color: 'var(--bs-success)' },
            { percentage: 20, color: 'var(--bs-warning)' },
            { percentage: 15, color: 'var(--bs-info)' },
            { percentage: 10, color: 'var(--bs-secondary)' }
        ];
        
        let currentAngle = 0;
        const paths = slices.map(slice => {
            const angle = (slice.percentage / 100) * 360;
            const startAngle = currentAngle;
            const endAngle = currentAngle + angle;
            
            const x1 = centerX + radius * Math.cos(startAngle * Math.PI / 180);
            const y1 = centerY + radius * Math.sin(startAngle * Math.PI / 180);
            const x2 = centerX + radius * Math.cos(endAngle * Math.PI / 180);
            const y2 = centerY + radius * Math.sin(endAngle * Math.PI / 180);
            
            const largeArc = angle > 180 ? 1 : 0;
            
            const path = `
                <path d="M ${centerX} ${centerY} L ${x1} ${y1} A ${radius} ${radius} 0 ${largeArc} 1 ${x2} ${y2} Z"
                      fill="${slice.color}" opacity="0.8">
                    <animate attributeName="opacity" from="0" to="0.8" dur="1s" fill="freeze"/>
                </path>
            `;
            
            currentAngle += angle;
            return path;
        }).join('');
        
        return paths;
    },

    // Create donut chart placeholder
    createDonutChartPlaceholder(width, height) {
        const centerX = width / 2;
        const centerY = height / 2;
        const outerRadius = Math.min(width, height) / 2 - 20;
        const innerRadius = outerRadius * 0.6;
        
        return `
            <circle cx="${centerX}" cy="${centerY}" r="${outerRadius}" 
                   fill="var(--bs-primary)" opacity="0.3"/>
            <circle cx="${centerX}" cy="${centerY}" r="${innerRadius}" 
                   fill="white"/>
            <text x="${centerX}" y="${centerY}" text-anchor="middle" 
                  dy="0.35em" font-size="24" font-weight="bold" fill="var(--bs-primary)">
                75%
            </text>
        `;
    },

    // Create default chart placeholder
    createDefaultChartPlaceholder(width, height) {
        return `
            <rect width="${width}" height="${height}" fill="var(--bs-light)" stroke="var(--bs-border-color)" stroke-width="1" rx="8"/>
            <text x="${width/2}" y="${height/2}" text-anchor="middle" dy="0.35em" 
                  font-size="16" fill="var(--bs-secondary)">
                Chart Placeholder
            </text>
        `;
    },

    // Update chart data
    updateChart(chartId, newData) {
        const chart = this.charts[chartId];
        if (!chart) {
            console.warn(`Chart with ID ${chartId} not found`);
            return;
        }
        
        // In a real implementation, this would update the actual chart
        console.log(`Updating chart ${chartId} with new data:`, newData);
        
        // Add a visual indicator that data has been updated
        const container = chart.container;
        container.style.opacity = '0.5';
        setTimeout(() => {
            container.style.opacity = '1';
        }, 300);
    },

    // Destroy chart
    destroyChart(chartId) {
        const chart = this.charts[chartId];
        if (chart) {
            chart.container.innerHTML = '';
            delete this.charts[chartId];
        }
    },

    // Get chart instance
    getChart(chartId) {
        return this.charts[chartId];
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    Charts.init();
});

// Expose to global namespace
window.TaskListProcessor.Charts = Charts;

export default Charts;
