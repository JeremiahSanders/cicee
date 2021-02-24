# What Files and Directories Does CICEE Require?

CICEE expects that a project repository is structured in the following manner.

* _Project repository root_
  * `ci/`
    * `bin/` - _Optional._ Conventional location of executable scripts intended to be run in a continuous integration environment. Examples: compile source script, run automated tests script, publish to repository script.
    * `ci.env` - _Optional._ Environment initialization script. Sourced during execution workflow.
    * `docker-compose.dependencies.yml` - _Optional._ Docker-compose services definitions providing dependencies for continuous integration environment. Examples: databases.
    * `docker-compose.project.yml` - _Optional._ Project-specific continuous integration environment definition. Supports declaring dependencies, specifying default environment variables, etc.
    * `Dockerfile` - _Required, unless `exec` is executed with `--image`._ - Dockerfile defining continuous integration environment.