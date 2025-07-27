# TaskListProcessor Web - Development Guide

## Summary

The TaskListProcessor Web application has been successfully updated to use npm for managing UI libraries with a modern frontend build system. The application now includes:

✅ **Bootstrap 5.3.3** - Complete CSS framework (no CDN dependency)
✅ **Bootstrap Icons 1.11.3** - Icon font (no CDN dependency)  
✅ **SCSS compilation** - Custom variables and component styling
✅ **JavaScript bundling** - Modular ES6 code with Rollup
✅ **Integrated build process** - Works with `dotnet build` and `dotnet publish`
✅ **Development workflow** - Watch mode for development
✅ **Production optimization** - Minified CSS and JS files

## Quick Start

### First Time Setup

```bash
cd TaskListProcessor.Web
npm install
npm run build
dotnet run
```

### Development Workflow

```bash
# Terminal 1: Start development watch mode
npm run dev

# Terminal 2: Run the application
dotnet run
```

## Build Commands

| Command | Description |
|---------|-------------|
| `npm install` | Install dependencies |
| `npm run build` | Build all assets (CSS + JS) |
| `npm run dev` | Watch mode for development |
| `npm run clean` | Clean generated files |
| `dotnet build` | Build .NET app (includes npm build) |

## File Structure

```
src/
├── scss/
│   ├── _variables.scss      # Bootstrap customizations
│   ├── _mixins.scss         # Reusable SCSS mixins
│   ├── _animations.scss     # CSS animations
│   ├── _components.scss     # Custom components
│   └── site.scss            # Main entry point
└── js/
    ├── main.js              # Entry point
    └── modules/
        ├── core.js          # Core functionality
        ├── ui-enhancements.js # UI improvements
        ├── charts.js        # Chart components
        └── streaming.js     # Real-time features

Generated Files:
wwwroot/
├── css/
│   ├── site.css            # Compiled CSS
│   ├── site.css.map        # Source map
│   └── site.min.css        # Minified CSS
└── js/
    ├── site.js             # Bundled JS
    ├── site.js.map         # Source map
    └── site.min.js         # Minified JS
```

## Customization

### SCSS Variables

Edit `src/scss/_variables.scss` to customize Bootstrap:

```scss
$primary: #your-brand-color;
$font-family-sans-serif: "Your Font";
```

### Custom Components

Add styles in `src/scss/_components.scss`:

```scss
.my-component {
  @include enhanced-card();
  // Custom styles
}
```

### JavaScript Modules

Create new modules in `src/js/modules/` and import in `main.js`.

## Features Included

### CSS Features

- Complete Bootstrap 5 framework
- Custom CSS variables and mixins
- Responsive design utilities
- Animation classes
- Print styles
- Custom scrollbar styling

### JavaScript Features

- Bootstrap JS components
- Modular architecture
- Global namespace (TaskListProcessor)
- UI enhancements (animations, tooltips)
- Notification system
- Chart placeholders
- Streaming functionality
- Form validation
- Accessibility enhancements

### Build Features

- SCSS compilation with source maps
- JavaScript bundling with Rollup
- Code minification for production
- Integrated with .NET build
- Watch mode for development
- Clean build artifacts

## Production Deployment

The build process is fully integrated with .NET:

```bash
dotnet publish
```

This will:

1. Install npm dependencies
2. Build and minify all assets
3. Include optimized files in publish output

## Browser Support

- Modern browsers (ES6+ support)
- Bootstrap 5 browser support
- Responsive design (mobile-first)

## Performance

- Single CSS file (includes Bootstrap + custom styles)
- Single JS file (includes Bootstrap + custom functionality)
- Minified files for production
- Source maps for debugging
- Tree-shaking for unused code

## Next Steps

1. **Test the application** - Run `dotnet run` and verify all functionality
2. **Customize styling** - Modify SCSS variables to match your brand
3. **Add components** - Create new UI components using the mixins
4. **Enhance functionality** - Add new JavaScript modules as needed
5. **Deploy** - Use `dotnet publish` for production deployment

The application is now ready for modern frontend development with a fully integrated build system!
