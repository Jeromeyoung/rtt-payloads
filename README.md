# rtt-payloads
Red Team Toolkit PIPELINE :rocket:

This is where we build .NET payloads and have them ready for execution for RTT

## How it works

1. Drop in your solution in the **TODO** directory
    *This can be done via git, commit, push or via the web browser.*
    *The pipeline will only run on push changes to the **TODO** directory.*

2. Built DLLs will live in the **OUT** folder as well as the S3 bucket: https://rtt-dlls.s3.us-east-2.amazonaws.com

3. Processed solution files will live in the **DONE** directory should someone ever need to see which one was last run.


## Things to condsider
When uploading a solution, the .SLN as well as a PROGRAM.CS file must exist.

theres more caveats... - tibi
to be added later
