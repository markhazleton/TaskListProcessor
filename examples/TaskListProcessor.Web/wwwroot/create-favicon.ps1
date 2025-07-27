# Simple favicon creation script
# Creates a basic 16x16 favicon for TaskListProcessor Web

# Base64 encoded simple ICO file (16x16, blue with white list items)
$faviconBase64 = @"
AAABAAEAEBAAAAEAIABoBAAAFgAAACgAAAAQAAAAIAAAAAEAIAAAAAAAAAQAABILAAASCwAAAAAAAAAAAAD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8AZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6v////8A////wP///8D///8A////wP///8D///8A////AP///wD///8A////AP///wD///8A////AP///wBmfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/////8D///8A////wP///8D///8A////wP///8D///8A////AP///wD///8A////AP///wD///8A////AGZ+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/////wP///8D///8A////wP///8D///8A////wP///8D///8A////AP///wD///8A////AP///wD///8AZn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v////8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wBmfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6v9mfur/Zn7q/2Z+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoAZn7qAGZ+6gBmfuoA////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==
"@

[System.IO.File]::WriteAllBytes("favicon.ico", [System.Convert]::FromBase64String($faviconBase64))
