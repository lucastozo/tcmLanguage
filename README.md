# TCM Language Interpreter

This is an interpreter for a custom assembly-style language I built while playing *Turing Complete*. The name TCM stands for Turing Complete Machine, which describes the 8-bit computer I designed in the game to run this language.

## Why This Exists

When you play *Turing Complete*, you eventually build your own CPU from basic logic gates. Each player's computer ends up being different based on their design choices. This interpreter is specifically tailored to my particular 8-bit computer implementation, so it won't work with other players' machines. Think of it as a custom toolchain for a very specific piece of hardware.

The language itself is Turing complete because the underlying machine is Turing complete, but it's constrained by the 8-bit architecture and 256-byte address space I built into my virtual CPU.

## How The Language Works

### Instruction Architecture

Every instruction in TCM follows a fixed 4-byte format that mirrors how my virtual CPU processes commands:

```
[OPCODE] [ARG1] [ARG2] [DEST]
```

This isn't arbitrary - it matches the instruction decoder I built in the game. The opcode tells the CPU what operation to perform, the two arguments provide the data or addresses to work with, and the destination specifies where to store the result.

For example, if you want to add two numbers:
```
ADD REG0 REG1 REG2
```

This translates to "take the value in register 0, add it to the value in register 1, and store the result in register 2." The CPU processes this by reading the opcode (ADD), fetching the values from the specified registers, performing the addition in the ALU, and writing the result back to the destination register.

### Understanding the Machine Model

The virtual machine simulates the exact hardware I built in *Turing Complete*. Here's what you're working with:

**Registers**: You have six 8-bit registers (REG0 through REG5) that serve as your fast storage. Register 5 has a special purpose - it controls the RAM address pointer, which is why you'll see it referenced as ADDRESS in many operations.

**RAM**: There are 256 bytes of memory addressable from 0 to 255. Since everything runs in 8-bit space, this is actually quite a lot of room to work with. The RAM address is controlled by REG5, so when you write to RAM, you're writing to whatever address is currently stored in that register.

**Stack**: The call stack handles subroutine calls and returns. When you call a subroutine, the return address gets pushed onto the stack automatically. When you return, it pops that address and jumps back.

**Output**: This is where your program can write results. In the context of the game, this might drive a display or other output device.

### Number Formats and Expressions

The language supports several ways to express values, which makes it more readable than raw machine code:

**Direct values**: You can write decimal numbers directly: `255`, `42`, `0`

**Binary notation**: You can write numbers in binary: `0b11111111` for 255, or `0b00001010` for 10. The binary format requires exactly 8 bits.

**Expressions**: You can write mathematical and bitwise expressions that get evaluated at parse time:
- `REG0 + 15` adds 15 to whatever is in register 0
- `0b11110000 | 0b00001111` performs a bitwise OR to get 255
- `255 / 2` divides 255 by 2 to get 127

The expression evaluator follows standard operator precedence: multiplication, division, and modulo first, then addition and subtraction, then bitwise operations. This means `2 + 3 * 4` equals 14, not 20.

### Immediate vs Register Arguments

One crucial concept is the difference between immediate values and register references. The CPU needs to know whether an argument is a literal value or a reference to a register/memory location.

This is handled through opcode modifiers. When you write:
```
ADD REG0 REG1 REG2
```

The CPU knows to read the values from registers 0 and 1. But if you write:
```
ADD|IMD_ARG2 REG0 15 REG2
```

The `|IMD_ARG2` modifier tells the CPU that the second argument (15) is an immediate value, not a register reference. This lets you mix literal values with register references in the same instruction.
Note that `|IMD_ARG2` is simply an operation OR with the value of `IMD_ARG2` (64), that is because the computer decides if the values are literals using the two most significant bits of the OPCODE.

### Keywords and Readability

Raw machine code would look like `0 0 1 2` for adding REG0 and REG1, storing in REG2. The keyword system makes this readable by providing aliases:
- `ADD` instead of `0`
- `REG0` instead of `0`
- `REG1` instead of `1`
- `RAM` instead of `128`

The keyword system also handles the immediate value modifiers. Instead of remembering that 64 means "second argument is immediate," you can write `IMD_ARG2`.

## Program Structure

### Constants

You can define named constants at the top of your program:
```
const ARRAY_SIZE 10
const MAX_VALUE 255
```

### Labels and Control Flow

Labels mark positions in your program for jumps and conditional branches:
```
label main_loop
    # ... some code ...
    IF_LES REG0 REG1 main_loop
```

When the condition is true (REG0 < REG1), execution jumps back to the main_loop label.

### Subroutines

Subroutines work like function calls in higher-level languages:
```
subroutine multiply_by_two
    ADD REG0 REG0 REG0  # Double the value in REG0
    RETURN BACK TO CALLER

# Calling the subroutine
IF_EQL|IMD_ARG3 1 1 multiply_by_two  # Always true condition
```

When you call a subroutine, the return address gets pushed onto the stack automatically. The `RETURN BACK TO CALLER` instruction pops that address and jumps back to where the subroutine was called.

## Memory Management

Since you're working with only 256 bytes of RAM, memory management becomes important for larger programs. The RAM address is controlled by REG5, so you'll often see patterns like:

```
COPY_CONST 0 TO ADDRESS     # Set RAM pointer to address 0
COPY_CONST 42 TO RAM        # Write 42 to RAM[0]
NEXT ADDRESS IN MEMORY      # Increment RAM pointer to 1
COPY_CONST 84 TO RAM        # Write 84 to RAM[1]
```

This gives you a way to build arrays and data structures in memory. The bubble sort example in the repository shows how to work with arrays using this addressing scheme.

## Running Programs

### Basic Execution

The simplest way to run a program is:
```bash
./tcmInterpreter-linux myprogram.tcm
```

This executes your program and shows the final output. If your program writes to OUTPUT, you'll see those values printed to the console.

### Debugging with Logs

For debugging, you'll want to see what's happening inside the virtual machine:
```bash
./tcmInterpreter-linux myprogram.tcm --log
```

This shows you each instruction as it executes, the state of all registers, RAM contents, and stack contents after each step. It's invaluable for understanding why your program isn't working as expected.

### Log Management

The execution logs can get quite large, especially for programs with loops. The interpreter includes several options to manage this:

```bash
# Save logs to a file
./tcmInterpreter-linux myprogram.tcm --log --write

# Limit log file size
./tcmInterpreter-linux myprogram.tcm --log --write --size 50

# Remove size limits. Careful: if your program is infinite (infinite loop) it can potentially fill all your disk
./tcmInterpreter-linux myprogram.tcm --log --write --no-limit
```

The default log size limit is 12MB, which is usually enough for debugging but prevents runaway programs from filling your disk.

## Example Walkthrough

Let's trace through a simple program to see how everything fits together:

```tcm
# Calculate 5 + 3 and output the result
COPY_CONST 5 TO REG0        # Put 5 in register 0
COPY_CONST 3 TO REG1        # Put 3 in register 1
ADD REG0 REG1 REG2          # Add them, store in register 2
MOV REG2 TO OUTPUT          # Copy result to output
```

When this runs:
1. The first instruction puts the literal value 5 into REG0
2. The second puts 3 into REG1
3. The ADD instruction reads REG0 and REG1, adds them, stores 8 in REG2
4. MOV copies the 8 from REG2 to OUTPUT (which prints it)

The value 8 should appear on your terminal after executing this program.

## Common Patterns

After working with this language, you'll notice certain patterns that come up frequently:

**Array iteration**: Setting up a loop to process each element in an array stored in RAM, using REG5 to control the memory address and another register to track your position.

**Conditional execution**: Using the conditional instructions (IF_EQL, IF_LES, etc.) not just for loops, but to implement if-then logic by jumping over blocks of code.

**Stack-based temporary storage**: Using the stack to temporarily save register values when you need to use those registers for other calculations.

## Limitations and Design Decisions

This language reflects the constraints of the 8-bit computer it runs on. You're limited to 256 bytes of RAM, 8-bit arithmetic (values 0-255), and six registers. These constraints force you to think carefully about algorithms and data structures, much like programming early microcomputers.

The instruction format is fixed at 4 bytes because that's how I designed the instruction decoder in the game. A more flexible format might be more efficient, but it would require a different CPU design.

The keywords is a balance between readability and the underlying machine representation. You could write programs using only numbers, but the keywords make the code much easier to understand and maintain.

## Getting Help

The Wiki contains additional examples and detailed explanations of specific features. If you're getting unexpected results, start by running your program with `--log` to see exactly what the virtual machine is doing at each step.

Remember that this interpreter is designed for one specific virtual CPU. If you're coming from other assembly languages, some of the conventions might seem unusual, but they reflect the particular design choices I made when building the CPU in *Turing Complete*.
