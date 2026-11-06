FROM mcr.microsoft.com/devcontainers/dotnet:2.3.4-10.0

RUN --mount=type=cache,target=/var/cache/apt \
    apt-get update && export DEBIAN_FRONTEND=noninteractive && \
    apt-get -y install --no-install-recommends \
    moreutils vim cargo-1.89 xxd

RUN mkdir -pv /workspaces/MerkurNota && chown -R vscode:vscode /workspaces/

USER vscode

RUN mkdir -pv /home/vscode/.nuget/NuGet/ \
    /home/vscode/.nuget/packages \
    /home/vscode/.vscode-server \
    /home/vscode/.config/opencode \
    /home/vscode/.local/share/opencode \
    && chown -R vscode:vscode \
    /home/vscode/.nuget \
    /home/vscode/.vscode-server

RUN [ -d /home/vscode/.oh-my-zsh ] || wget https://github.com/robbyrussell/oh-my-zsh/raw/master/tools/install.sh -O - | zsh

ENV SSL_CERT_DIR="/home/vscode/.aspnet/dev-certs/trust:/usr/lib/ssl/certs"

RUN dotnet tool install --global dotnet-ef
RUN dotnet dev-certs https --trust 
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
RUN cargo-1.89 install git-cliff

ENV PATH="/home/vscode/.cargo/bin:$PATH"

# Create a temporary directory for vscode user - this is needed because tmpfs on /tmp broke coding agents.
RUN mkdir -p /home/vscode/.tmp
ENV TMPDIR="/home/vscode/.tmp"

USER root
