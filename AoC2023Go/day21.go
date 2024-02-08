package main

import "fmt"

// Part 1: 3532
// Part 2: solved in c# project

type Day21Sequence struct {
	steps        int
	visitedCount int
}

type Day21Node struct {
	x         int
	y         int
	stepCount int
	prevX     int
	prevY     int
}

type Day21 struct {
	BaseDay

	initialData    [][]bool
	startX         int
	startY         int
	nodes          []*Day21Node
	dataRows       [][]bool
	finalNodeRows  [][]bool
	visitedRows    [][]int
	toVisitRows    [][]int
	finalNodeCount int

	sequenceCounts     []Day21Sequence
	lookForSpecificPos bool
	lookForX           int
	lookForY           int
	lookForXs          []int
	lookForXIndex      int

	dataWidth  int
	dataHeight int

	width  int
	height int

	firstLine  bool
	lineNumber int
}

func (d *Day21) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day21) parseLine(line string) {
	defer d.incLineNumber()

	lineLen := len(line)
	if lineLen == 0 {
		return
	}
	if d.firstLine {
		d.dataWidth = len(line)
		d.firstLine = false
	}

	row := make([]bool, d.dataWidth)

	for n, e := range line {
		if e == '#' {
			row[n] = true
		} else if e == 'S' {
			d.startX = n
			d.startY = len(d.initialData)
		}
	}
	d.initialData = append(d.initialData, row)
	d.dataHeight = len(d.initialData)
}

func (d *Day21) initialiseData(multiple int) {
	d.width = d.dataWidth * multiple
	d.height = d.dataHeight * multiple
	d.finalNodeRows = nil
	d.visitedRows = nil
	d.toVisitRows = nil
	for n := 0; n < multiple; n++ {
		for y := 0; y < d.dataHeight; y++ {
			dataRow := make([]bool, d.width)
			for o := 0; o < multiple; o++ {
				for x := 0; x < d.dataWidth; x++ {
					dataRow[o*d.dataWidth+x] = d.initialData[y][x]
				}
			}
			d.dataRows = append(d.dataRows, dataRow)
			finalRow := make([]bool, d.width)
			visited := make([]int, d.width)
			toVisit := make([]int, d.width)
			d.finalNodeRows = append(d.finalNodeRows, finalRow)
			d.visitedRows = append(d.visitedRows, visited)
			d.toVisitRows = append(d.toVisitRows, toVisit)
		}
	}

}

func (d *Day21) incLineNumber() {
	d.lineNumber += 1
}

func (d *Day21) outputData() {
	fmt.Printf("%d x %d\n", len(d.dataRows[0]), len(d.dataRows))
	for y, row := range d.dataRows {
		for x, e := range row {
			if y == d.startY && x == d.startX {
				if d.finalNodeRows[y][x] {
					fmt.Printf("$")
				} else {
					fmt.Printf("S")
				}
			} else if e {
				fmt.Printf("#")
			} else {
				if d.finalNodeRows[y][x] {
					fmt.Printf("O")
				} else {
					fmt.Printf(".")
				}
			}
		}
		fmt.Println()
	}
}

func (d *Day21) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day21) executePart1() {
	d.initialiseData(1)
	fmt.Println("\nExecute part 1")
	stepCount := 100
	d.queueNode(d.startX, d.startY, d.startX, d.startY, stepCount)
	cycle := 0
	for len(d.nodes) > 0 {
		// if cycle%10000 == 0 {
		// 	fmt.Printf("Queue size %d sc %d lsc %d\n", len(d.nodes), d.nodes[0].stepCount, d.nodes[len(d.nodes)-1].stepCount)
		// }

		d.processNode()
		cycle++
	}

	fmt.Println()
	fmt.Printf("\nFinal node count %d\n", d.finalNodeCount)
}

func (d *Day21) executePart2() {
	fmt.Println("\nExecute part 2")
	d.initialiseData(21)
	d.startX += 10 * d.dataWidth
	d.startY += 10 * d.dataHeight

	d.lookForXs = append(d.lookForXs, d.startX-66)
	d.lookForXs = append(d.lookForXs, d.startX-d.dataWidth*1-66)
	d.lookForXs = append(d.lookForXs, d.startX-d.dataWidth*2-66)
	d.lookForXs = append(d.lookForXs, d.startX-d.dataWidth*3-66)
	d.lookForY = d.startY
	fmt.Printf("%d %d %d  , %d\n", d.lookForXs[0], d.lookForXs[1], d.lookForXs[2], d.lookForY)
	d.lookForX = d.lookForXs[d.lookForXIndex]
	d.lookForXIndex++

	d.lookForSpecificPos = true

	stepCount := 10000000000
	d.queueNode(d.startX, d.startY, d.startX, d.startY, stepCount)
	cycle := 0
	for len(d.nodes) > 0 {
		d.processNode()
		cycle++
	}

	var result int64
	if d.lookForSpecificPos {
		var totalStepsNeeded int64 = 26501365
		for i := 0; i < 3; i++ {
			var term int64 = int64(d.sequenceCounts[i].visitedCount)
			for j := 0; j < 3; j++ {
				if j != i {
					term = term * (totalStepsNeeded - int64(d.sequenceCounts[j].steps)) /
						int64(d.sequenceCounts[i].steps-d.sequenceCounts[j].steps)
				}
			}
			result += term
		}
	}
	fmt.Printf("Result is %v\n", result)

	fmt.Println()
	fmt.Printf("\nFinal node count %d\n", d.finalNodeCount)
}

func (d *Day21) countPlotsToFillForStepCount(stepCount int) int {
	count := 0
	oddStepCount := stepCount%2 == 1
	for y := 0; y < d.height; y++ {
		xSt := 1
		if oddStepCount {
			xSt = 0
		}
		for x := xSt; x < d.width; x += 2 {
			if !d.dataRows[y][x] {
				count++
			}
		}
		oddStepCount = !oddStepCount
	}
	return count
}

func (d *Day21) queueNode(prevX int, prevY int, x int, y int, stepCount int) {
	//	fmt.Printf("(%d,%d) %d\n", y, x, len(d.toVisitRows))
	if d.toVisitRows[y][x] > 0 {
		return
	}
	node := &Day21Node{prevX: x, prevY: y, x: x, y: y, stepCount: stepCount}
	d.nodes = append(d.nodes, node)
	d.toVisitRows[y][x] = stepCount
}

func (d *Day21) dequeueNode() *Day21Node {
	toReturn := d.nodes[0]
	d.nodes = d.nodes[1:]
	return toReturn
}

func (d *Day21) processNode() {
	node := d.dequeueNode()
	x := node.x
	y := node.y

	if d.lookForSpecificPos && x == d.lookForX && y == d.lookForY {
		fmt.Printf("At pos (%d,%d), count is %d\n", x, y, d.finalNodeCount)
		addSequence := Day21Sequence{d.startX - d.lookForX, d.finalNodeCount}
		d.sequenceCounts = append(d.sequenceCounts, addSequence)
		if d.lookForXIndex == len(d.lookForXs) {
			d.nodes = nil
			return
		}
		d.lookForX = d.lookForXs[d.lookForXIndex]
		fmt.Printf(" Looking for (%d,%d)\n", d.lookForX, d.lookForY)
		d.lookForXIndex++
	}
	d.toVisitRows[y][x] = 0

	if node.stepCount%2 == 0 && !d.finalNodeRows[y][x] {
		d.finalNodeRows[y][x] = true
		d.finalNodeCount++
	}

	if node.stepCount == 0 {
		return
	}
	if x > 0 && x-1 != node.prevX && !d.dataRows[y][x-1] {
		d.queueNode(x, y, x-1, y, node.stepCount-1)
	}
	if x < d.width-1 && x+1 != node.prevX && !d.dataRows[y][x+1] {
		d.queueNode(x, y, x+1, y, node.stepCount-1)
	}
	if y > 0 && y-1 != node.prevY && !d.dataRows[y-1][x] {
		d.queueNode(x, y, x, y-1, node.stepCount-1)
	}
	if y < d.height-1 && y+1 != node.prevY && !d.dataRows[y+1][x] {
		d.queueNode(x, y, x, y+1, node.stepCount-1)
	}
}
