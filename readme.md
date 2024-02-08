### Advent of Code 2023 solutions

See https://adventofcode.com/2023 for the questions. Note the answers that are in the comments at the top of each file, are the answers for the puzzle input I was given. Other elf-helpers will have been different puzzle inputs.

December was obviously a lot of fun, with lots of expletives about why the elves don't seem to like Christmas so much.

The solutions are mixture of C# and Go. Day 1->11 was implemented in C# and then I flicked to go for a while. The day 1 Go implementation is only for the first part of the puzzle, my intention was to reimplement all solutions in Go, but the earlier puzzles are pretty basic and I couldn't face it.

Later on some of the solutions are in C# and some are in Go, depending on how I was feeling that day.

I managed to complete the puzzles by the 27th, as Christmas got in the way of graph theory (Kargers algorithm on the 25th) and hailstone management (seemingly multiple simultaenous equations on the 24th, but reduced a few line intersections).

## Execution

Both the Go and C# solutions rely on a directory structure, C:\Misc\AdventOfCode2023\Day{n}\ where each days directory should contain test data in the form of "test.txt", "test2.txt", "test3.txt" or "data.txt" for the puzzle input. Note, no "test1.txt". Alter the starting set up in either program.cs or Aoc2023.go to run a particular day/part/test.

## Will I do it again?

Probably not. It takes a lot of time, and I do not feel that the elves are pullng their Christmas-weight. However, there is a puzzle that was recommended in the reddit forum for AoC, back in 2016, that I look forward to tackling: 

https://adventofcode.com/2016/day/11