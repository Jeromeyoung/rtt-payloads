# SharpDomainSpray


SharpDomainSpray is a very simple password spraying tool written in .NET. It takes a password then finds users in the domain and attempts to authenticate to the domain with that given password.

## Usage 

cmd.exe
```
C:\> SharpDomainSpray.exe test12345!

User: Administrator Password is: test12345!

User: TestUser Password is: test12345!
```
Cobalt Strike
```
> execute-assembly /path/to/SharpDomainSpray.exe test12345!

User: Administrator Password is: test12345!

User: TestUser Password is: test12345!
```
## Author

Tom Kallo
