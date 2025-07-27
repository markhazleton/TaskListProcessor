// Import Bootstrap JavaScript and expose it globally
import * as bootstrap from 'bootstrap';
window.bootstrap = bootstrap;

// Import existing functionality (we'll move the current site.js content here)
import './modules/core';
import './modules/ui-enhancements';
import './modules/charts';
import './modules/streaming';
import './modules/prism';

// Initialize the application when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    if (window.TaskListProcessor && typeof window.TaskListProcessor.init === 'function') {
        window.TaskListProcessor.init();
    }
    
    // Initialize PrismJS for syntax highlighting
    if (window.TaskListProcessor && window.TaskListProcessor.PrismJS) {
        window.TaskListProcessor.PrismJS.init();
    }
});
