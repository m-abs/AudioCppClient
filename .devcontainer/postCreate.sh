#!/bin/bash

BASH_SOURCE_DIR="$(dirname "${BASH_SOURCE[0]}")"

cd "$BASH_SOURCE_DIR/.."

# Set up the container's own GPG agent for signed commits.
# The container is shared the host's restricted gpg-agent socket, which forbids
# signing; remove it and start a local agent. Importing the secret key still
# requires a passphrase prompt and must be done in a real terminal if missing.
chown vscode:vscode .
chmod 700 "$HOME/.gnupg"
rm -f "$HOME/.gnupg/S.gpg-agent"* "$HOME/.gnupg/S.keyboxd"*
printf 'pinentry-mode loopback\n' > "$HOME/.gnupg/gpg.conf"
printf 'allow-loopback-pinentry\n' > "$HOME/.gnupg/gpg-agent.conf"
chmod 600 "$HOME/.gnupg/gpg.conf" "$HOME/.gnupg/gpg-agent.conf"
gpgconf --launch gpg-agent
if ! gpg --list-secret-keys --with-colons 2>/dev/null | grep -q '^sec'; then
    echo ""
    echo "GPG: no secret key in container."
    echo "For signed commits, export your GPG key on the hostand run the setup script:"
    echo "  bin/dev/setup_gpg_signing.sh nota-gpg-key.asc   # passphrase prompt, see README"
fi

# Run local post-create script if it exists.
if [ -f .devcontainer/postCreate.local.sh ]; then
    echo "Running local post-create script."
    bash .devcontainer/postCreate.local.sh
else
    echo "No local post-create script found."
fi
