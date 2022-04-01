# Initialize a Project Repository

Initializes a project repository directory with [suggested CICEE files][init] and [continuous integration scripts][template-init]. Optionally includes [CICEE CI library][template-lib].

This combines the following CICEE commands' results:

* [`init`][init]
* [`template init`][template-init]
* (optionally) [`template lib`][template-lib]

```bash
$ cicee init repository --help
Description:
  Initialize project repository. Creates suggested CICEE files and continuous integration scripts. Optionally includes CICEE
  CI library.

Usage:
  cicee init repository [options]

Options:
  -p, --project-root <project-root> (REQUIRED)  Project repository root directory [default:
                                                /c/code/TEMP_PROJECT_FOR_CICEE_TEST]
  -i, --image <image>                           Base CI image for $PROJECT_ROOT/ci/Dockerfile.
  -f, --force                                   Force writing files. Overwrites files which already exist. [default: False]
  -l, --ci-lib                                  Initialize project CI with CICEE execution library. Supports 'cicee
                                                exec'-like behavior without CICEE installation. [default: False]
  -s, --shell <bash>                            Shell template. []
  -?, -h, --help                                Show help and usage information
```

[init]: ./initialize.md
[template-init]: ./template-init.md
[template-lib]: ./template-lib.md
