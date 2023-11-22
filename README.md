# MirrorManager

utilities to help manage mirrors and subscriptions

## Usage

```bash
./MirrorMnager --help
Description:
  Manages local chia data layer mirrors and subscriptions.

Usage:
  MirrorManager [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  list-all         List all stores and their mirrors.
  unmirror-all     Unmirrors all stores.
  unsubscribe-all  Unsubscribes from all stores.
```

### Options

```bash
./MirrorMnager list-all --help
Description:
  List all stores and their mirrors.

Usage:
  MirrorManager list-all [options]

Options:
  -o, --ours      Whether to only list our mirrors. [default: True]
  -?, -h, --help  Show help and usage information
```

```bash
./MirrorMnager unmirror-all --help
Description:
  Unmirrors all stores.

Usage:
  MirrorManager unmirror-all [options]

Options:
  -f, --fee <fee>  Fee to use for each removal transaction. [default: 0]
  -?, -h, --help   Show help and usage information
```

```bash
./MirrorMnager unsubscribe-all --help
Description:
  Unsubscribes from all stores.

Usage:
  MirrorManager unsubscribe-all [options]

Options:
  -r, --retain    Whether to retain files when unsubscribing. [default: False]
  -?, -h, --help  Show help and usage information
```
