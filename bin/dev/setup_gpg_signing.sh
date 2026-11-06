#!/bin/bash

# Set up signed commits in the dev container for the given exported secret key.
#
# The container gets the host's restricted gpg-agent socket forwarded in, which
# forbids signing/importing ("Forbidden" / "No pinentry"). This removes it and
# starts the container's own agent.
#
# The key id is read from the .asc file, so only input needed is the passphrase
# prompt (in a real terminal) during the import.
#
# Usage: setup_gpg_signing.sh [/path/to/key.asc]
# Without a key file only the agent setup is done.

set -e

KEYFILE="$1"

if [ -n "${KEYFILE}" ] && [ ! -f "${KEYFILE}" ]; then
    echo "Key file not found: ${KEYFILE}" >&2
    exit 1
fi

chmod 700 "$HOME/.gnupg"
rm -f "$HOME/.gnupg/S.gpg-agent"* "$HOME/.gnupg/S.keyboxd"*
printf 'pinentry-mode loopback\n' > "$HOME/.gnupg/gpg.conf"
printf 'allow-loopback-pinentry\n' > "$HOME/.gnupg/gpg-agent.conf"
chmod 600 "$HOME/.gnupg/gpg.conf" "$HOME/.gnupg/gpg-agent.conf"
gpgconf --launch gpg-agent

if [ -z "${KEYFILE}" ]; then
    echo "GPG agent ready. To enable signed commits, run with the exported key file:"
    echo "  ${0} /path/to/exported-secret-key.asc"
    exit 0
fi

KEY_ID=$(gpg --show-keys --with-colons "${KEYFILE}" | awk -F: '/^sec/ {print $5; exit}')

if [ -z "${KEY_ID}" ]; then
    echo "No secret key found in: ${KEYFILE}" >&2
    exit 1
fi

echo "Found secret key: ${KEY_ID}"

echo "Importing key (enter the key's passphrase when prompted)..."
gpg --import "${KEYFILE}"

git config --global user.signingKey "${KEY_ID}"
git config --global commit.gpgsign true

echo "Signed commits enabled for key ${KEY_ID}. Try: git commit"
