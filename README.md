[![.NET](https://github.com/Datalayer-Storage/MirrorManager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Datalayer-Storage/MirrorManager/actions/workflows/dotnet.yml)
[![CodeQL](https://github.com/Datalayer-Storage/MirrorManager/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/Datalayer-Storage/MirrorManager/actions/workflows/github-code-scanning/codeql)

# MirrorManager

utilities to help manage chia data layer mirrors and subscriptions

## Usage

```bash
./MirrorManager --help
Description:
  Manages local chia data layer mirrors and subscriptions.

Usage:
  MirrorManager [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  check            Verify that a mirror is accessible.
  list-all         List all stores and their mirrors.
  unmirror-all     Unmirrors all stores.
  unsubscribe-all  Unsubscribes from all stores.
```

### Options

```bash
./MirrorManager check --help
Description:
  Verify that a mirror is accessible.

Usage:
  MirrorManager check [options]

Options:
  -h, --host <host>  The host address to check. Omit to check the local mirror. []
  -?, -h, --help     Show help and usage information
```

```bash
./MirrorManager list-all --help
Description:
  List all stores and their mirrors.

Usage:
  MirrorManager list-all [options]

Options:
  -o, --ours      Whether to only list our mirrors. [default: True]
  -?, -h, --help  Show help and usage information
```

```bash
./MirrorManager unmirror-all --help
Description:
  Unmirrors all stores.

Usage:
  MirrorManager unmirror-all [options]

Options:
  -f, --fee <fee>  Fee to use for each removal transaction. [default: 0]
  -?, -h, --help   Show help and usage information
```

```bash
./MirrorManager unsubscribe-all --help
Description:
  Unsubscribes from all stores.

Usage:
  MirrorManager unsubscribe-all [options]

Options:
  -r, --retain    Whether to retain files when unsubscribing. [default: False]
  -?, -h, --help  Show help and usage information
```
