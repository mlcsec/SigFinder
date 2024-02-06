# SigFinder

Identify binaries with Authenticode digital signatures signed to an internal CA/domain. Useful for enumerating Windows directory paths referenced in WDAC policies or searching for internal LOB applications.

```
C:\Tools> SigFinder.exe
Usage: SigFinder.exe <directoryPath> [-ignore <string1>,<string2>,...] [-recursive] [-domain <domain>]
```
<br>

Optional flags:
- `-ignore` - ignore all certificates containing supplied string/comma seperated strings
- `-recursive` - recursively check for certificates from the provided directory path
- `-domain` - only display certificates containing the the domain keyword

<br>

![sigfinder](https://github.com/mlcsec/SigFinder/assets/47215311/0cf078a1-ebeb-4757-a457-4fef4240267f)

<br>

# NOTE

Add quotes to directory paths containing spaces and either REMOVE the trailing backslash or ADD a backslash:
```
beacon> executeInline-Assembly --dotnetassembly C:\Tools\SigFinder.exe "C:\Program Files" -ignore microsoft
beacon> executeInline-Assembly --dotnetassembly C:\Tools\SigFinder.exe "C:\Program Files\\" -ignore microsoft
```
Your beacon WILL DIE if you don't.
