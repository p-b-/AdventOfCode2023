package main

import (
	"fmt"
	"strconv"
)

type CrucibleDirection int

func (d CrucibleDirection) String() string {
	switch d {
	case Right:
		return "Right"
	case Left:
		return "Left"
	case Down:
		return "Down"
	case Up:
		return "Up"
	}
	return "unknownDir"
}

const (
	Right = iota
	Down
	Left
	Up
)

type Day17PointInTime struct {
	x         int
	y         int
	direction CrucibleDirection
}

type Day17 struct {
	BaseDay

	lowestCost int64
	costs      map[Day17PointInTime]int64

	rows [][]int

	width  int
	height int

	firstLine  bool
	lineNumber int
}

func (d *Day17) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day17) parseLine(line string) {
	if d.firstLine {
		d.width = len(line)
		d.firstLine = false
	}
	if len(line) > 0 {
		d.addRow(line)
		d.height = len(d.rows)
	}

	d.lineNumber = d.lineNumber + 1
}

func (d *Day17) addRow(row string) {
	newRow := make([]int, 0, len(row))
	for n := 0; n < len(row); n++ {
		slice := row[n : n+1]
		cost, _ := strconv.Atoi(slice)
		newRow = append(newRow, cost)
	}
	d.rows = append(d.rows, newRow)
}

func (d *Day17) execute(part int) {
	fmt.Printf("%d x %d\n", d.width, d.height)
	d.costs = make(map[Day17PointInTime]int64)
	for _, r := range d.rows {
		for _, n := range r {
			fmt.Printf("%d  ", n)
		}
		fmt.Println()
	}
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day17) executePart1() {
	x, y := 0, 0
	d.lowestCost = -1

	fmt.Printf("*** Excute part 1: right\n")
	d.recurse(x, y, Right, 0, "", " ")
	fmt.Printf("*** Excute part 1: down\n")
	d.recurse(x, y, Down, 0, "", " ")
	fmt.Printf("Lowest cost is %d\n", d.lowestCost)
}

func (d *Day17) directionInfo(dir CrucibleDirection) (CrucibleDirection, CrucibleDirection, int, int) {
	if dir == Down {
		return Right, Left, 0, 1
	} else if dir == Right {
		return Down, Up, 1, 0
	} else if dir == Up {
		return Right, Left, 0, -1
	} else if dir == Left {
		return Down, Up, -1, 0
	}
	panic("Unknown direction")
}

func (d *Day17) recurse(subpathXStart int, subpathYStart int, subpathDirection CrucibleDirection, currentCost int64, instructions string, padding string) (int64, bool) {
	fmt.Printf("%sRecursed (%d,%d)\n%s%s\n", padding, subpathXStart, subpathYStart, padding, instructions)
	if d.lowestCost != -1 && currentCost > d.lowestCost {
		return -1, false
	}
	// if currentCost > 60 {
	// 	panic("costs to high")
	// }

	cit := Day17PointInTime{subpathXStart, subpathYStart, subpathDirection}

	if storedSubcost, ok := d.costs[cit]; ok {
		return storedSubcost, true
	}

	nextDir1, nextDir2, xStepMultiplier, yStepMultiplier := d.directionInfo(subpathDirection)

	directionToUse := nextDir1
	log := false
	var minSubCost int64 = -1
	methodInstructions := instructions
	for splitPathIndex := 0; splitPathIndex < 2; splitPathIndex++ {
		var thisSubPathCost int64 = 0
		for step := 1; step <= 3; step++ {
			log = false
			if subpathXStart == 0 && subpathYStart == 0 {
				log = true
			}
			nextX := subpathXStart + step*xStepMultiplier
			nextY := subpathYStart + step*yStepMultiplier

			if nextX < 0 || nextX >= d.width || nextY < 0 || nextY >= d.height {
				if log {
					fmt.Println()
				}
				break
			}
			subInstruction := subpathDirection.String()
			instructions += subInstruction
			thisSubPathCost += int64(d.rows[nextY][nextX])
			if d.lowestCost != -1 && currentCost+thisSubPathCost > d.lowestCost {
				break
			}

			if nextX == d.width-1 && nextY == d.height-1 {
				// Reached destination
				potentialLowestCost := currentCost + thisSubPathCost
				fmt.Printf("%s*** Reached destination\n", padding)
				fmt.Printf("%s    Cost: %d\n", padding, potentialLowestCost)
				fmt.Printf("%s    Dirs: %s\n", padding, instructions)
				if d.lowestCost == -1 || potentialLowestCost < d.lowestCost {
					d.lowestCost = potentialLowestCost
				}
				d.costs[cit] = thisSubPathCost
				return thisSubPathCost, true
			}

			subcost, success := d.recurse(nextX, nextY, directionToUse, currentCost+thisSubPathCost, instructions, padding+" ")

			if success && subcost > 0 {
				if d.lowestCost == -1 || currentCost+thisSubPathCost+subcost < d.lowestCost {
					fmt.Printf("%sSetting lowest cost to %d\n", padding, currentCost+thisSubPathCost+subcost)
					d.lowestCost = currentCost + thisSubPathCost + subcost
				}
				if minSubCost == -1 || thisSubPathCost+subcost < minSubCost {
					minSubCost = thisSubPathCost + subcost
				}
			} else {
				break
			}
		}
		directionToUse = nextDir2
		instructions = methodInstructions
	}

	d.costs[cit] = minSubCost
	return minSubCost, true
}

func (d *Day17) executePart2() {
	// Implemented in C#
}
