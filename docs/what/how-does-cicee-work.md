# How does CICEE work?

CICEE's core functionality, in its `exec` command, performs the following workflow. The workflow is executed by `bash` script. See [What Files and Directories Does CICEE Require?][] for more details about the files the worklow uses.

1. _If the project repository contains an intialization script&hellip;_ `source` the project's continuous integration initialization script, pulling its exported variables into the workflow environment.
2. _If the project repository contains a continuous integration Dockerfile&hellip;_ `docker build` the project's continuous integration environment Docker image.
3. Use `docker-compose` to create a temporary continuous integration environment, pulling any dependencies the project requires.
4. Execute a user-provided (`docker-compose`) `entrypoint` and/or `command` in the continuous integration environment.
5. Tear down the continuous integration environment.

[What Files and Directories Does CICEE Require?]: ../use/project-structure.md