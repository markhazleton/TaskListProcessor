/**
 * TaskListProcessor Web - PrismJS Module
 * Provides syntax highlighting for code blocks
 */

// Import PrismJS core and components
import Prism from 'prismjs';

// Import language components for documentation
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-json';
import 'prismjs/components/prism-xml-doc';
import 'prismjs/components/prism-bash';
import 'prismjs/components/prism-powershell';
import 'prismjs/components/prism-yaml';
import 'prismjs/components/prism-markdown';

// Import plugins
import 'prismjs/plugins/line-numbers/prism-line-numbers';
import 'prismjs/plugins/copy-to-clipboard/prism-copy-to-clipboard';
import 'prismjs/plugins/toolbar/prism-toolbar';

const PrismJS = {
    init() {
        this.setupCodeBlocks();
        this.enhanceCodeBlocks();
        console.log('PrismJS syntax highlighting initialized');
    },

    // Setup existing code blocks for PrismJS
    setupCodeBlocks() {
        // Find existing code blocks and prepare them for PrismJS
        const codeBlocks = document.querySelectorAll('.code-block pre code, pre code');
        
        codeBlocks.forEach(codeBlock => {
            // Determine language from context or class
            let language = this.detectLanguage(codeBlock);
            
            // Add language class if not present
            if (!codeBlock.className.includes('language-')) {
                codeBlock.className = `language-${language} ${codeBlock.className}`.trim();
            }
            
            // Add line numbers if parent has line-numbers class
            const pre = codeBlock.parentElement;
            if (pre && pre.tagName === 'PRE') {
                pre.classList.add('line-numbers');
            }
        });
        
        // Highlight all code blocks
        Prism.highlightAll();
    },

    // Detect language from code content
    detectLanguage(codeBlock) {
        const content = codeBlock.textContent || codeBlock.innerText || '';
        
        // C# patterns
        if (content.includes('using System') || 
            content.includes('var ') || 
            content.includes('new ') ||
            content.includes('public class') ||
            content.includes('namespace ') ||
            content.match(/\b(var|using|public|private|class|interface|namespace)\b/)) {
            return 'csharp';
        }
        
        // JSON patterns
        if (content.trim().startsWith('{') && content.trim().endsWith('}') ||
            content.trim().startsWith('[') && content.trim().endsWith(']')) {
            try {
                JSON.parse(content.trim());
                return 'json';
            } catch (e) {
                // Not valid JSON, continue detection
            }
        }
        
        // PowerShell patterns
        if (content.includes('$') && content.includes('-') ||
            content.includes('Get-') || content.includes('Set-') ||
            content.includes('New-') || content.includes('Remove-')) {
            return 'powershell';
        }
        
        // Bash/Shell patterns
        if (content.includes('#!/bin/bash') || content.includes('npm run') ||
            content.includes('dotnet ') || content.includes('cd ') ||
            content.match(/^[\w-]+@[\w-]+:/)) {
            return 'bash';
        }
        
        // XML/HTML patterns
        if (content.includes('<') && content.includes('>') &&
            content.match(/<\/?[\w-]+[^>]*>/)) {
            return 'markup';
        }
        
        // YAML patterns
        if (content.includes(':') && content.match(/^\s*[\w-]+:\s*[\w-]/m)) {
            return 'yaml';
        }
        
        // Default to csharp for our documentation
        return 'csharp';
    },

    // Enhance code blocks with Bootstrap 5 styling
    enhanceCodeBlocks() {
        const codeBlocks = document.querySelectorAll('pre[class*="language-"]');
        
        codeBlocks.forEach(pre => {
            // Wrap in Bootstrap card if not already wrapped
            if (!pre.closest('.code-block-enhanced')) {
                this.wrapCodeBlock(pre);
            }
        });
    },

    // Wrap code block in Bootstrap card
    wrapCodeBlock(pre) {
        const wrapper = document.createElement('div');
        wrapper.className = 'code-block-enhanced card border-0 shadow-sm mb-4';
        
        const header = document.createElement('div');
        header.className = 'card-header bg-light border-0 py-2 px-3';
        
        const language = this.getLanguageFromClass(pre.className);
        const languageLabel = this.getLanguageLabel(language);
        
        header.innerHTML = `
            <div class="d-flex justify-content-between align-items-center">
                <small class="text-muted fw-medium">
                    <i class="bi bi-code-slash me-1"></i>${languageLabel}
                </small>
                <button class="btn btn-sm btn-outline-secondary copy-btn" type="button" title="Copy to clipboard">
                    <i class="bi bi-clipboard"></i>
                </button>
            </div>
        `;
        
        const body = document.createElement('div');
        body.className = 'card-body p-0';
        
        // Insert wrapper before pre element
        pre.parentNode.insertBefore(wrapper, pre);
        
        // Move pre into wrapper
        wrapper.appendChild(header);
        wrapper.appendChild(body);
        body.appendChild(pre);
        
        // Add copy functionality
        const copyBtn = header.querySelector('.copy-btn');
        if (copyBtn) {
            copyBtn.addEventListener('click', () => this.copyCode(pre, copyBtn));
        }
    },

    // Get language from class name
    getLanguageFromClass(className) {
        const match = className.match(/language-(\w+)/);
        return match ? match[1] : 'text';
    },

    // Get human-readable language label
    getLanguageLabel(language) {
        const labels = {
            'csharp': 'C#',
            'json': 'JSON',
            'javascript': 'JavaScript',
            'typescript': 'TypeScript',
            'html': 'HTML',
            'css': 'CSS',
            'scss': 'SCSS',
            'bash': 'Bash',
            'powershell': 'PowerShell',
            'yaml': 'YAML',
            'markdown': 'Markdown',
            'xml': 'XML',
            'markup': 'HTML'
        };
        return labels[language] || language.toUpperCase();
    },

    // Copy code to clipboard
    async copyCode(pre, button) {
        const code = pre.querySelector('code');
        const text = code.textContent || code.innerText;
        
        try {
            await navigator.clipboard.writeText(text);
            
            // Update button state
            const icon = button.querySelector('i');
            const originalClass = icon.className;
            icon.className = 'bi bi-check-lg';
            button.classList.remove('btn-outline-secondary');
            button.classList.add('btn-success');
            
            setTimeout(() => {
                icon.className = originalClass;
                button.classList.remove('btn-success');
                button.classList.add('btn-outline-secondary');
            }, 2000);
            
        } catch (err) {
            console.error('Failed to copy code:', err);
            
            // Fallback: select text
            const selection = window.getSelection();
            const range = document.createRange();
            range.selectNodeContents(code);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }
};

// Expose to global namespace
window.TaskListProcessor = window.TaskListProcessor || {};
window.TaskListProcessor.PrismJS = PrismJS;

export default PrismJS;
