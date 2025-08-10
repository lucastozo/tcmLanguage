# TCM Language Interpreter

This is an interpreter for a custom assembly-style language I built while playing *Turing Complete*. The name TCM stands for Turing Complete Machine, which describes the 8-bit computer I designed in the game to run this language.

## Why This Exists

When you play *Turing Complete*, you eventually build your own CPU from basic logic gates. Each player's computer ends up being different based on their design choices. This interpreter is specifically tailored to my particular 8-bit computer implementation, so it won't work with other players' machines. Think of it as a custom toolchain for a very specific piece of hardware.

The language itself is Turing complete because the underlying machine is Turing complete, but it's constrained by the 8-bit architecture and 256-byte address space I built into my virtual CPU.

For more info on how to start using it, check the [Wiki](https://github.com/lucastozo/tcmLanguage/wiki).
