package main

import "fmt"

type Day interface {
	readFile(part int)
	execute(part int)
	SetFilePath(filepath string)
}

func NewDay(dayNumber int) Day {
	return NewDayWithTestParameters(dayNumber, true, 0)
}

func NewDayWithTestParameters(dayNumber int, testing bool, testPart int) Day {
	var toReturn Day
	switch dayNumber {
	case 11:
		toReturn = new(Day11)
	case 12:
		toReturn = new(Day12)
	case 13:
		toReturn = new(Day13)
	case 14:
		toReturn = new(Day14)
	case 15:
		toReturn = new(Day15)
	case 16:
		toReturn = new(Day16)
	case 17:
		toReturn = new(Day17)
	case 19:
		toReturn = new(Day19)
	case 20:
		toReturn = new(Day20)
	case 21:
		toReturn = new(Day21)
	case 22:
		toReturn = new(Day22)
	case 23:
		toReturn = new(Day23)
	}

	var filename = "data.txt"
	if testing && testPart == 0 {
		filename = "test.txt"
	} else if testing {
		filename = fmt.Sprintf("test%d.txt", testPart)
	}
	toReturn.SetFilePath(fmt.Sprintf("C:\\Misc\\AdventOfCode2023\\Day%d\\%s", dayNumber, filename))
	return toReturn
}
