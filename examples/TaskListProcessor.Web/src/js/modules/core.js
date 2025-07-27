/**
 * TaskListProcessor Web - Core Module
 * Main application initialization and global utilities
 */

// Create global namespace
window.TaskListProcessor = window.TaskListProcessor || {};

// Core functionality
const Core = {
    // Private variables
    isInitialized: false,
    
    // Initialize the application
    init() {
        if (this.isInitialized) return;
        
        this.setupGlobalHandlers();
        this.enhanceAccessibility();
        this.initializeTooltips();
        this.isInitialized = true;
        
        console.log('TaskListProcessor Web Core initialized');
    },

    // Setup global event handlers
    setupGlobalHandlers() {
        // Handle Bootstrap modal events
        document.addEventListener('shown.bs.modal', function(event) {
            // Focus first input in modal
            const firstInput = event.target.querySelector('input, textarea, select');
            if (firstInput) {
                firstInput.focus();
            }
        });

        // Handle form submissions
        document.addEventListener('submit', function(event) {
            const form = event.target;
            if (form.classList.contains('needs-validation')) {
                event.preventDefault();
                event.stopPropagation();
                
                if (form.checkValidity()) {
                    // Form is valid, proceed with submission
                    Core.showLoading(form);
                }
                
                form.classList.add('was-validated');
            }
        });

        // Handle clicks on elements with data-toggle attributes
        document.addEventListener('click', function(event) {
            const element = event.target.closest('[data-bs-toggle]');
            if (element && element.dataset.bsToggle === 'tooltip') {
                // Only prevent default if it's not a navigation link
                if (!element.href && !element.closest('a[href]')) {
                    event.preventDefault();
                }
            }
        });
    },

    // Enhance accessibility
    enhanceAccessibility() {
        // Add skip link functionality
        const skipLink = document.querySelector('.skip-link');
        if (skipLink) {
            skipLink.addEventListener('click', function(event) {
                event.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.focus();
                    target.scrollIntoView();
                }
            });
        }

        // Enhance keyboard navigation
        document.addEventListener('keydown', function(event) {
            // Escape key closes modals
            if (event.key === 'Escape') {
                const openModal = document.querySelector('.modal.show');
                if (openModal) {
                    const modal = bootstrap.Modal.getInstance(openModal);
                    if (modal) modal.hide();
                }
            }
        });
    },

    // Initialize Bootstrap tooltips
    initializeTooltips() {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => 
            new bootstrap.Tooltip(tooltipTriggerEl)
        );
    },

    // Show loading state
    showLoading(element) {
        const button = element.querySelector('button[type="submit"]');
        if (button) {
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="bi bi-arrow-clockwise animate-spin"></i> Loading...';
            button.disabled = true;
            
            // Store original text for later restoration
            button.dataset.originalText = originalText;
        }
    },

    // Hide loading state
    hideLoading(element) {
        const button = element.querySelector('button[type="submit"]');
        if (button && button.dataset.originalText) {
            button.innerHTML = button.dataset.originalText;
            button.disabled = false;
            delete button.dataset.originalText;
        }
    },

    // Show notification
    showNotification(message, type = 'info', duration = 5000) {
        // Create toast element
        const toastHtml = `
            <div class="toast align-items-center text-bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi bi-${this.getIconForType(type)} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // Get or create toast container
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        // Add toast to container
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = toastContainer.lastElementChild;
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        
        toast.show();

        // Remove element after it's hidden
        toastElement.addEventListener('hidden.bs.toast', function() {
            this.remove();
        });
    },

    // Get icon for notification type
    getIconForType(type) {
        const icons = {
            'success': 'check-circle-fill',
            'danger': 'exclamation-triangle-fill',
            'warning': 'exclamation-triangle-fill',
            'info': 'info-circle-fill',
            'primary': 'info-circle-fill',
            'secondary': 'info-circle-fill'
        };
        return icons[type] || 'info-circle-fill';
    },

    // Utility: Debounce function
    debounce(func, wait, immediate) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func.apply(this, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(this, args);
        };
    },

    // Utility: Throttle function
    throttle(func, limit) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }
};

// Expose Core functionality
window.TaskListProcessor.Core = Core;
window.TaskListProcessor.init = Core.init.bind(Core);
window.TaskListProcessor.showNotification = Core.showNotification.bind(Core);
window.TaskListProcessor.showLoading = Core.showLoading.bind(Core);
window.TaskListProcessor.hideLoading = Core.hideLoading.bind(Core);

export default Core;
