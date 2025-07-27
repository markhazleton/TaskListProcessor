# TaskListProcessor Web - Frontend Build System

This project now uses npm and modern frontend tooling for managing UI libraries and assets.

## Overview

The frontend build system uses:

- **Bootstrap 5** - Modern CSS framework
- **Bootstrap Icons** - Icon font
- **SCSS** - CSS preprocessing with variables and mixins
- **Rollup** - JavaScript bundling
- **npm scripts** - Build automation

## Build Process

### Automatic Build (Integrated with .NET)

The build process is automatically integrated with the .NET build system. When you run:

```bash
dotnet build
dotnet publish
```

The frontend assets will be automatically built.

### Manual Build Commands

Install dependencies:

```bash
npm install
```

Build all assets:

```bash
npm run build
```

Watch for changes during development:

```bash
npm run dev
# or
npm run watch
```

Clean build artifacts:

```bash
npm run clean
```

### Individual Build Commands

Build CSS only:

```bash
npm run build:css
```

Build JavaScript only:

```bash
npm run build:js
```

## Project Structure

```
src/
├── scss/
│   ├── _variables.scss      # Bootstrap variable overrides
│   ├── _mixins.scss         # Custom SCSS mixins
│   ├── _animations.scss     # CSS animations
│   ├── _components.scss     # Custom components
│   └── site.scss           # Main SCSS entry point
└── js/
    ├── main.js              # Main JavaScript entry point
    └── modules/
        ├── core.js          # Core functionality
        ├── ui-enhancements.js # UI enhancements
        ├── charts.js        # Chart functionality
        └── streaming.js     # Real-time streaming
```

## Generated Assets

The build process generates:

- `wwwroot/css/site.css` - Compiled CSS with Bootstrap
- `wwwroot/css/site.min.css` - Minified CSS
- `wwwroot/js/site.js` - Bundled JavaScript with Bootstrap
- `wwwroot/js/site.min.js` - Minified JavaScript

## Customization

### SCSS Variables

Customize Bootstrap variables in `src/scss/_variables.scss`:

```scss
$primary: #your-color;
$font-family-sans-serif: "Your Font", sans-serif;
```

### Custom Components

Add custom styles in `src/scss/_components.scss` using the provided mixins:

```scss
.my-component {
  @include enhanced-card();
  @include fadeInUp();
}
```

### JavaScript Modules

Add new functionality by creating modules in `src/js/modules/` and importing them in `main.js`.

## Dependencies

### Runtime Dependencies

- **Bootstrap 5.3.3** - CSS framework
- **Bootstrap Icons 1.11.3** - Icon font

### Development Dependencies

- **Sass** - SCSS compilation
- **Rollup** - JavaScript bundling
- **Terser** - JavaScript minification
- **CleanCSS** - CSS minification

## Development Workflow

1. Make changes to SCSS files in `src/scss/`
2. Make changes to JavaScript files in `src/js/`
3. Run `npm run dev` to watch for changes
4. Build and test with `dotnet run`

## Production Build

For production deployment, ensure all assets are built:

```bash
npm run build
dotnet publish
```

The published application will include all optimized frontend assets.
