/**
 * TaskListProcessor Web - UI Enhancements Module
 * Provides enhanced UI functionality and interactions
 */

const UIEnhancements = {
    init() {
        this.setupCardAnimations();
        this.setupButtonEnhancements();
        this.setupFormEnhancements();
        this.setupScrollEnhancements();
        console.log('UI Enhancements initialized');
    },

    // Setup card hover animations
    setupCardAnimations() {
        const cards = document.querySelectorAll('.card-enhanced, .card-feature');
        
        cards.forEach(card => {
            card.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-5px)';
            });
            
            card.addEventListener('mouseleave', function() {
                this.style.transform = 'translateY(0)';
            });
        });
    },

    // Setup button enhancements
    setupButtonEnhancements() {
        // Add ripple effect to buttons
        const buttons = document.querySelectorAll('.btn');
        
        buttons.forEach(button => {
            button.addEventListener('click', function(e) {
                const ripple = document.createElement('span');
                const rect = this.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;
                
                ripple.style.width = ripple.style.height = size + 'px';
                ripple.style.left = x + 'px';
                ripple.style.top = y + 'px';
                ripple.classList.add('ripple');
                
                this.appendChild(ripple);
                
                setTimeout(() => {
                    ripple.remove();
                }, 600);
            });
        });
    },

    // Setup form enhancements
    setupFormEnhancements() {
        // Floating labels enhancement
        const floatingInputs = document.querySelectorAll('.form-floating input, .form-floating textarea');
        
        floatingInputs.forEach(input => {
            // Check if input has value on load
            if (input.value) {
                input.classList.add('has-value');
            }
            
            input.addEventListener('input', function() {
                if (this.value) {
                    this.classList.add('has-value');
                } else {
                    this.classList.remove('has-value');
                }
            });
        });

        // Auto-resize textareas
        const textareas = document.querySelectorAll('textarea[data-auto-resize]');
        textareas.forEach(textarea => {
            textarea.addEventListener('input', function() {
                this.style.height = 'auto';
                this.style.height = this.scrollHeight + 'px';
            });
        });
    },

    // Setup scroll enhancements
    setupScrollEnhancements() {
        // Scroll to top button
        const scrollToTopBtn = this.createScrollToTopButton();
        
        // Show/hide scroll to top button
        window.addEventListener('scroll', window.TaskListProcessor.Core.throttle(() => {
            if (window.scrollY > 300) {
                scrollToTopBtn.classList.add('show');
            } else {
                scrollToTopBtn.classList.remove('show');
            }
        }, 100));

        // Smooth scroll for anchor links
        document.addEventListener('click', function(e) {
            const link = e.target.closest('a[href^="#"]');
            if (link && link.getAttribute('href') !== '#') {
                e.preventDefault();
                const target = document.querySelector(link.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });

        // Reveal animations on scroll
        this.setupScrollReveal();
    },

    // Create scroll to top button
    createScrollToTopButton() {
        const button = document.createElement('button');
        button.className = 'btn btn-primary scroll-to-top position-fixed';
        button.innerHTML = '<i class="bi bi-arrow-up"></i>';
        button.setAttribute('aria-label', 'Scroll to top');
        button.style.cssText = `
            bottom: 2rem;
            right: 2rem;
            width: 3rem;
            height: 3rem;
            border-radius: 50%;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s ease;
            z-index: 1000;
        `;
        
        button.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });
        
        // Add CSS for show state
        const style = document.createElement('style');
        style.textContent = `
            .scroll-to-top.show {
                opacity: 1 !important;
                visibility: visible !important;
            }
            .scroll-to-top:hover {
                transform: translateY(-2px);
            }
        `;
        document.head.appendChild(style);
        document.body.appendChild(button);
        
        return button;
    },

    // Setup scroll reveal animations
    setupScrollReveal() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in-up');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        // Observe elements with reveal animation
        const revealElements = document.querySelectorAll('.card, .feature-item, .stat-item, .reveal-on-scroll');
        revealElements.forEach(el => {
            observer.observe(el);
        });
    },

    // Create loading overlay
    createLoadingOverlay(container) {
        const overlay = document.createElement('div');
        overlay.className = 'loading-overlay position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center';
        overlay.style.cssText = `
            background-color: rgba(255, 255, 255, 0.8);
            z-index: 10;
            backdrop-filter: blur(2px);
        `;
        overlay.innerHTML = `
            <div class="text-center">
                <div class="loading mb-3"></div>
                <div class="text-muted">Loading...</div>
            </div>
        `;
        
        container.style.position = 'relative';
        container.appendChild(overlay);
        
        return overlay;
    },

    // Remove loading overlay
    removeLoadingOverlay(container) {
        const overlay = container.querySelector('.loading-overlay');
        if (overlay) {
            overlay.remove();
        }
    },

    // Copy to clipboard functionality
    copyToClipboard(text, successMessage = 'Copied to clipboard!') {
        navigator.clipboard.writeText(text).then(() => {
            window.TaskListProcessor.showNotification(successMessage, 'success');
        }).catch(() => {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            window.TaskListProcessor.showNotification(successMessage, 'success');
        });
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    UIEnhancements.init();
});

// Expose to global namespace
window.TaskListProcessor.UIEnhancements = UIEnhancements;

export default UIEnhancements;
