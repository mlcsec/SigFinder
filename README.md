# SigFinder

Identify binaries with Authenticode digital signatures signed to an internal CA/domain. Useful for enumerating Windows directory paths referenced in WDAC policies or searching for internal applications.

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

![image](https://github-production-user-asset-6210df.s3.amazonaws.com/47215311/299970382-af927899-3f45-472d-8f66-95cd16ec777f.png?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAVCODYLSA53PQK4ZA%2F20240126%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20240126T212656Z&X-Amz-Expires=300&X-Amz-Signature=6bed0d65645267861f32becd3971d42399025b2bbdb1756ff26213c38f1ec759&X-Amz-SignedHeaders=host&actor_id=47215311&key_id=0&repo_id=747919370)

<br>

# NOTE

Add quotes to directory paths containing spaces and either REMOVE the trailing backslash or ADD a backslash:
```
beacon> executeInline-Assembly --dotnetassembly C:\Tools\SigFinder.exe "C:\Program Files" -ignore microsoft
beacon> executeInline-Assembly --dotnetassembly C:\Tools\SigFinder.exe "C:\Program Files\\" -ignore microsoft
```
Your beacon WILL DIE if you don't.
