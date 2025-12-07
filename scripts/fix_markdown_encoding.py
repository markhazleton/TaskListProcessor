#!/usr/bin/env python3
"""
Fix Markdown Encoding Issues
Replaces corrupted Unicode characters (displayed as '?') with proper emojis and box-drawing characters.
Usage: python fix_markdown_encoding.py [--dry-run]
"""

import os
import sys
import re
from pathlib import Path
from typing import List, Tuple

# Define replacements: (pattern, replacement, description)
REPLACEMENTS: List[Tuple[str, str, str]] = [
    # Box-drawing characters (multi-char patterns first)
    (r'\?\?\?', '???', 'Tree branch with line'),
    (r'\?   \?', '?   ?', 'Tree vertical + branch'),
    (r'\?   ', '?   ', 'Tree vertical with spaces'),
    
    # Emojis with context (to avoid replacing wrong '?' marks)
    (r'\? \*\*Teaches\*\*', '?? **Teaches**', 'Books emoji'),
    (r'\? \*\*Demonstrates\*\*', '?? **Demonstrates**', 'Light bulb emoji'),
    (r'\? \*\*Engages\*\*', '?? **Engages**', 'Handshake emoji'),
    (r'\? \*\*Guides\*\*', '?? **Guides**', 'Graduation cap emoji'),
    (r'\? \*\*Inspires\*\*', '? **Inspires**', 'Sparkles emoji'),
    (r'extraordinary! \?\?', 'extraordinary! ??', 'Rocket emoji'),
    (r'### \?\? \*\*Vision Statement\*\*', '### ?? **Vision Statement**', 'Target emoji'),
    (r'## \?\? \*\*Vision Statement\*\*', '### ?? **Vision Statement**', 'Target emoji (alternate)'),
    
    # Status emojis with common patterns
    (r'(?<=\|)\s*\?\?\s*(?=\|)', ' ?? ', 'Target in table'),
    (r'- \? Complete', '- ? Complete', 'Check mark'),
    (r'- \? Planned', '- ? Planned', 'Hourglass'),
    (r'- \? Partial', '- ?? Partial', 'Yellow circle'),
]

def fix_file(file_path: Path, dry_run: bool = False) -> Tuple[bool, int]:
    """
    Fix encoding issues in a single markdown file.
    Returns: (changed, replacement_count)
    """
    try:
        # Read file with UTF-8 encoding
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        replacement_count = 0
        
        # Apply replacements
        for pattern, replacement, description in REPLACEMENTS:
            matches = len(re.findall(pattern, content))
            if matches > 0:
                content = re.sub(pattern, replacement, content)
                replacement_count += matches
                if matches > 0:
                    print(f"  - {description}: {matches} replacements")
        
        # Check if content changed
        if content != original_content:
            if not dry_run:
                # Write back with UTF-8 BOM for better compatibility
                with open(file_path, 'w', encoding='utf-8-sig') as f:
                    f.write(content)
            return True, replacement_count
        
        return False, 0
        
    except Exception as e:
        print(f"  ERROR: {e}")
        return False, 0

def main():
    """Main execution function."""
    dry_run = '--dry-run' in sys.argv or '-n' in sys.argv
    
    print("\n=== Markdown Encoding Fix ===")
    if dry_run:
        print("Running in DRY RUN mode (no changes will be made)\n")
    else:
        print("Running in LIVE mode (files will be modified)\n")
    
    # Find all markdown files in docs directory
    docs_dir = Path('docs')
    if not docs_dir.exists():
        print(f"Error: {docs_dir} directory not found")
        return 1
    
    md_files = list(docs_dir.rglob('*.md'))
    print(f"Found {len(md_files)} markdown files\n")
    
    fixed_count = 0
    total_replacements = 0
    
    # Process each file
    for file_path in sorted(md_files):
        print(f"Processing: {file_path.relative_to(docs_dir)}")
        changed, replacements = fix_file(file_path, dry_run)
        
        if changed:
            fixed_count += 1
            total_replacements += replacements
            status = "Would fix" if dry_run else "? Fixed"
            print(f"  {status}: {replacements} replacements")
        else:
            print(f"  No changes needed")
        print()
    
    # Summary
    print("=== Summary ===")
    print(f"Files processed: {len(md_files)}")
    print(f"Files fixed: {fixed_count}")
    print(f"Total replacements: {total_replacements}")
    
    if dry_run and fixed_count > 0:
        print("\n??  Run without --dry-run to apply changes")
    elif fixed_count > 0:
        print("\n? Complete! Verify changes with:")
        print("  git diff docs/")
    
    return 0

if __name__ == '__main__':
    sys.exit(main())
