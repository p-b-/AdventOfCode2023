package main

// Part 1 not 4418, not 11108
// Part 1 8419
// Part 2 160500973317706

import (
	"fmt"
	"strconv"
	"strings"
)

type Day12Line struct {
	OriginalPattern string
	Pattern         string
	Numbers         []int
}

type Day12 struct {
	BaseDay
	lineNumber    int64
	firstLine     bool
	maxWildcards  int
	lines         []Day12Line
	multiplyLines bool
}

func (d *Day12) readFile(part int) {
	if part == 0 {
		d.multiplyLines = false
	} else {
		d.multiplyLines = true
	}
	fmt.Printf("Reading file %v\n", d.filepath)
	d.firstLine = true
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day12) execute(part int) {
	var sum int64 = 0

	d.removeAnyStartCertainies()
	d.outputPuzzleInput()

	for i, line := range d.lines {
		combinations := d.process(line)
		fmt.Printf("Line :%d gave %d combinations\n", i, combinations)
		sum += combinations
	}

	fmt.Printf("sum is %v\n", sum)
}

func (d *Day12) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	day12Line := Day12Line{}

	count := strings.Count(line, "?")
	if count > d.maxWildcards {
		d.maxWildcards = count
	}
	fmt.Printf("%s : %d ?s\n", line, count)

	splitOnSpace := strings.Fields(line)
	pattern := splitOnSpace[0]
	numbers := splitOnSpace[1]
	if d.multiplyLines {
		pattern = pattern + "?" + pattern + "?" + pattern + "?" + pattern + "?" + pattern
		numbers = numbers + "," + numbers + "," + numbers + "," + numbers + "," + numbers
	}
	day12Line.OriginalPattern = pattern

	day12Line.Pattern = d.preprocessPatternRemovingDots(pattern)
	day12Line.Numbers = d.preprocessNumbers(numbers)
	d.lines = append(d.lines, day12Line)

	d.lineNumber = d.lineNumber + 1
}

func (d *Day12) preprocessPatternRemovingDots(line string) string {
	for len(line) > 0 && line[0] == '.' {
		line = line[1:]
	}
	for len(line) > 0 && line[len(line)-1] == '.' {
		line = line[0 : len(line)-1]
	}
	return line
}

func (d *Day12) preprocessNumbers(line string) []int {
	var toReturn = make([]int, 0, 10)
	numbers := strings.Split(line, ",")
	for _, nAsStr := range numbers {
		nAsInteger, _ := strconv.Atoi(nAsStr)
		toReturn = append(toReturn, nAsInteger)

	}
	return toReturn
}

func (d *Day12) outputPuzzleInput() {
	for _, day12line := range d.lines {
		fmt.Printf("%s\n", day12line.Pattern)
		for _, n := range day12line.Numbers {
			fmt.Printf(" %d\n", n)
		}
	}
}

func (d *Day12) OutputPuzzleLine(lineIndex int) {
	fmt.Printf("%s\n", d.lines[lineIndex].Pattern)
	for _, n := range d.lines[lineIndex].Numbers {
		fmt.Printf(" %d\n", n)
	}
}

func (d *Day12) removeAnyStartCertainies() {
	for dayIndex, day12Line := range d.lines {
		pattern := day12Line.Pattern
		// Loop, first checking the start of the string for things to remove, then the end of the string
		//  Finish looping when an iteration does not change anything.
		innerLoop := true
		for innerLoop {
			innerLoop = false
			count := day12Line.Numbers[0]
			if len(pattern) > 0 && pattern[0] == '#' {
				pattern = pattern[count:]
				if len(pattern) > 0 && pattern[0] == '?' {
					// Cannot have a ? immediately following a valid row of #
					pattern = pattern[1:]
					innerLoop = true
				}
				pattern = d.preprocessPatternRemovingDots((pattern))
				day12Line.Pattern = pattern
				day12Line.Numbers = day12Line.Numbers[1:]
				if len(day12Line.Numbers) == 0 {
					day12Line.Pattern = pattern
					break
				}
			}

			count = day12Line.Numbers[len(day12Line.Numbers)-1]

			if len(pattern) > 0 && pattern[len(pattern)-1] == '#' {
				// Pattern ends with a # - a span is anchored there
				if len(pattern) == count {
					// Remove all pattern
					pattern = ""
					day12Line.Numbers = nil
					// No point in continuing loop, pattern has gone
					innerLoop = false
				} else {
					innerLoop = true
					// Remove the span
					pattern = pattern[:len(pattern)-count]
					// Was the span preceeded by a ? ?
					if pattern[len(pattern)-1] == '?' {
						// Cannot have a ? immediately preceeding a valid row of #.  Remove it
						pattern = pattern[:len(pattern)-1]
					}
					// Remove any dots at the end (also removes them from the start)
					pattern = d.preprocessPatternRemovingDots((pattern))
					// Remove span from numbers
					day12Line.Numbers = day12Line.Numbers[:len(day12Line.Numbers)-1]
					if len(day12Line.Numbers) == 0 {
						// No more numbers, save pattern
						day12Line.Pattern = pattern
						break
					}
				}
				day12Line.Pattern = pattern
			}
		}

		d.lines[dayIndex] = day12Line
	}
}

func (d *Day12) process(line Day12Line) int64 {
	fmt.Printf("processing line :%s:\n", line.Pattern)
	if len(line.Numbers) == 00 {
		return 1
	}
	var count int64 = 0
	stopLoopHere := false
	visited := make(map[string]int64)

	var subCount int64
	for startPos := 0; startPos < len(line.Pattern); startPos++ {
		if line.Pattern[startPos] == '#' {
			// First pattern is anchored here, cannot go past it
			stopLoopHere = true
		}
		pattern := line.Pattern[startPos:]

		if len(pattern) > 0 {
			// Count how many combinatons can start from here
			subCount, _ = d.recurse(pattern, line.Numbers, visited)
			count += subCount
		}
		if stopLoopHere {
			break
		}
	}

	return count
}

func (d *Day12) recurse(pattern string, numbers []int, visited map[string]int64) (int64, bool) {
	// Determine if this pattern+numbers has been seen before
	key := pattern + fmt.Sprintf("%v", numbers)
	if returnCount, ok := visited[key]; ok {
		// This pattern+numbers has been processed before, return previously calculated value
		return returnCount, true
	}
	// This pattern+numbers currently has a value of 0.  If the method exits due to reasons such
	//  as, the pattern doesn't fit, the spans try to overrite .'s, then this is recorded for
	//  future invocations for this pattern+numbers.
	visited[key] = 0
	spanLength := numbers[0]
	// Remove leading .s
	if pattern[0] == '.' {
		// Can't put a number span when a . is.  Return true to keep iterating across pattern.
		return 0, true
	}

	if len(pattern) < spanLength {
		// Pattern cannot fit, return false, as iterating further won't fit either
		return 0, false
	}

	// Ensure writing a span to this point in the pattern won't overwrite any .'s
	for i := 0; i < spanLength; i++ {
		if pattern[i] == '.' {
			// Span caannot overwrite a .
			// Return and iterate to next position
			return 0, true
		}
	}
	// Is this span contiguous to another number?
	if len(pattern) > spanLength && pattern[spanLength] == '#' {
		// Cannot butt against another number
		// Return and iterate to next position
		return 0, true
	}
	if len(numbers) == 1 {
		// Last number being considers, are there any further numbers in the pattern?
		if strings.Contains(pattern[spanLength:], "#") {
			// There are futher numbers in the pattern, but no number to match.
			//  Return and allow iteration to another position
			return 0, true
		}
		// No further numbers, this pattern search found 1 matching combination
		visited[key] = 1
		return 1, true
	}

	// Remove found span from pattern, and potentially a .
	if len(pattern) > spanLength {
		pattern = pattern[spanLength+1:]
	} else {
		pattern = ""
	}

	success := false
	stopLoopHere := false
	var subCount int64
	var count int64

	// To get here, there must be more numbers to match against the remaining pattern.  Do this
	//  via recursion to this method
	for i := 0; i < len(pattern); i++ {
		if pattern[i] == '#' {
			// The next number has to be anchored here - moving past this # would leave it
			//  unmatched against a number span
			stopLoopHere = true
		}
		// Get a count of matching patterns from this point
		subCount, success = d.recurse(pattern[i:], numbers[1:], visited)
		if !success {
			// No match possible, record current count against the pattern+numbers parameters, for future
			//  invocations
			visited[key] = count
			return count, true
		}
		// Increase count for this level of recursion
		count += subCount
		if stopLoopHere {
			break
		}
	}
	// Record current count against the pattern+numbers parameters, for future invocations
	visited[key] = count
	return count, true
}
