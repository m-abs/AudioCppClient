#!/bin/bash

sudo chown -v $USER:$USER $HOME/.omp | grep -v retained
sudo chown -Rv $USER:$USER $HOME/.omp/{data,profile,native,cache,plugins} | grep -v retained

mkdir -pv $HOME/.omp/{data,profile,native,cache,plugins} || echo

if command -v bun >/dev/null 2>&1; then
    echo "Bun is already installed."
else
    curl -fsSL https://bun.sh/install | bash
    
    echo 'export PATH=$HOME/.bun/bin:$PATH' >> $HOME/.zshrc
    export PATH=$HOME/.bun/bin:$PATH
fi

if command -v omp >/dev/null 2>&1; then
    echo "OMP is already installed."
else
    curl -fsSL https://omp.sh/install | sh
    
    echo 'export PATH=$HOME/.local/bin:$PATH' >> $HOME/.zshrc
    export PATH=$HOME/.local/bin:$PATH
fi

if command -v opencode >/dev/null 2>&1; then
    echo "OpenCode is already installed."
else
    curl -fsSL https://opencode.ai/v2/install | bash
    echo 'export PATH=$HOME/.opencode/bin:$PATH' >> $HOME/.zshrc
    export PATH=$HOME/.opencode/bin:$PATH
fi

if command -v codegraph >/dev/null 2>&1; then
    echo "CodeGraph is already installed."
else
    npm i -g @colbymchenry/codegraph
fi

gh extension install github/gh-stack

omp plugin marketplace add Notalib/omp-multi-agent-review || echo -n
omp plugin install multi-review@omp-multi-review || echo -n
omp plugin install npm:@vndv/pi-codegraph || echo -n
