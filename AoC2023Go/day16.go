package main

import "fmt"

type tileType int
type beamDirection int

// Part 1: 7798
// Part 2: 8026

func (d beamDirection) String() string {
	switch d {
	case beamRight:
		return "Right"
	case beamLeft:
		return "Left"
	case beamDown:
		return "Down"
	case beamUp:
		return "Up"
	}
	return "unknownDir"
}

const (
	emptyTile tileType = iota
	mirrorBottomLeftToUpperRight
	mirrorTopLeftToBottomRight
	verticalSplitter
	horizSplitter
)

const (
	beamRight beamDirection = 1 << iota
	beamDown
	beamLeft
	beamUp
)

type Day16 struct {
	BaseDay

	tileRows    [][]tileType
	visitedRows [][]beamDirection
	energyRows  [][]int
	width       int
	height      int

	firstLine  bool
	lineNumber int

	debugCounter int
}

func (d *Day16) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day16) parseLine(line string) {
	if d.firstLine {
		d.width = len(line)
		d.firstLine = false
	}
	if len(line) > 0 {
		d.addRow(line)
		d.height = len(d.tileRows)
	}

	d.lineNumber = d.lineNumber + 1
}

func (d *Day16) clearWorkings() {
	d.energyRows = nil
	d.visitedRows = nil
	for y := 0; y < d.height; y++ {
		newEnergyRow := make([]int, 0, d.width)
		newVisitedRow := make([]beamDirection, 0, d.height)
		for x := 0; x < d.width; x++ {
			newEnergyRow = append(newEnergyRow, 0)
			newVisitedRow = append(newVisitedRow, 0)
		}

		d.energyRows = append(d.energyRows, newEnergyRow)
		d.visitedRows = append(d.visitedRows, newVisitedRow)
	}
}

func (d *Day16) addRow(line string) {
	newRow := make([]tileType, 0, len(line))
	newEnergyRow := make([]int, 0, len(line))
	newVisitedRow := make([]beamDirection, 0, len(line))

	for _, c := range line {
		t := emptyTile
		switch c {
		case '|':
			t = verticalSplitter
		case '-':
			t = horizSplitter
		case '/':
			t = mirrorBottomLeftToUpperRight
		case '\\':
			t = mirrorTopLeftToBottomRight
		}
		newRow = append(newRow, t)
		newEnergyRow = append(newEnergyRow, 0)
		newVisitedRow = append(newVisitedRow, 0)
	}
	d.tileRows = append(d.tileRows, newRow)
	d.energyRows = append(d.energyRows, newEnergyRow)
	d.visitedRows = append(d.visitedRows, newVisitedRow)
}

func (d *Day16) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day16) outputPlan() {
	fmt.Printf("Outputing plan %d x %d:\n", d.width, d.height)
	fmt.Printf("   ")
	for x := 0; x < d.width; x++ {
		fmt.Printf("%02d ", x)
	}
	fmt.Println()
	for y := 0; y < d.height; y++ {
		fmt.Printf("%02d ", y)
		for x := 0; x < d.width; x++ {
			t := d.tileRows[y][x]
			c := '.'
			switch t {
			case verticalSplitter:
				c = '|'
			case horizSplitter:
				c = '-'
			case mirrorBottomLeftToUpperRight:
				c = '/'
			case mirrorTopLeftToBottomRight:
				c = '\\'
			}
			fmt.Printf("%c  ", c)
		}
		fmt.Println()
	}
}

func (d *Day16) outputEnergy() {
	fmt.Printf("Outputing energy %d x %d:\n", d.width, d.height)

	fmt.Printf("   ")
	for x := 0; x < d.width; x++ {
		fmt.Printf("%02d ", x)
	}
	fmt.Println()
	for y := 0; y < d.height; y++ {
		fmt.Printf("%02d ", y)
		for x := 0; x < d.width; x++ {
			n := d.energyRows[y][x]
			if n == 0 {
				fmt.Printf(".  ")
			} else {
				fmt.Printf("%d  ", n)
			}
		}
		fmt.Println()
	}
}

func (d *Day16) alterBeamDirMirrorTopLeftToBottomRight(dir beamDirection) beamDirection {
	switch dir {
	case beamRight:
		return beamDown
	case beamLeft:
		return beamUp
	case beamUp:
		return beamLeft
	case beamDown:
		return beamRight
	}
	return dir
}

func (d *Day16) alterBeamDirMirrorBottomLeftToUpperRight(dir beamDirection) beamDirection {
	switch dir {
	case beamRight:
		return beamUp
	case beamLeft:
		return beamDown
	case beamUp:
		return beamRight
	case beamDown:
		return beamLeft
	}
	return dir
}

func (d *Day16) moveBeamInDirection(dir beamDirection, x, y int) (int, int) {
	switch dir {
	case beamRight:
		return x + 1, y
	case beamLeft:
		return x - 1, y
	case beamUp:
		return x, y - 1
	case beamDown:
		return x, y + 1
	}
	return x, y
}

func (d *Day16) traceEnergyBeam(dir beamDirection, x, y int) {
	for {
		//fmt.Printf("%d ", d.debugCounter)
		if x < 0 || x >= d.width || y < 0 || y >= d.height {
			//	fmt.Printf("Cannot move in direction %v to (%d,%d)\n", dir, x, y)
			return
		}

		if d.visitedRows[y][x]&dir == dir {
			return
		}

		d.debugCounter++
		// if d.debugCounter > 500 {
		// 	return
		// }
		d.energyRows[y][x]++
		d.visitedRows[y][x] = d.visitedRows[y][x] | dir
		t := d.tileRows[y][x]
		if t == mirrorTopLeftToBottomRight {
			//fmt.Printf("\\ mirror, dir %v to ", dir)
			dir = d.alterBeamDirMirrorTopLeftToBottomRight(dir)
			//	fmt.Printf("dir %v ", dir)
		} else if t == mirrorBottomLeftToUpperRight {
			//			fmt.Printf("/ mirror, dir %v to ", dir)
			dir = d.alterBeamDirMirrorBottomLeftToUpperRight(dir)
			//fmt.Printf("dir %v ", dir)
		} else if t == verticalSplitter {
			if dir == beamLeft || dir == beamRight {
				//fmt.Printf("Splitting beam in dir %v to go up and down at (%d,%d)\n", dir, x, y)
				d.traceEnergyBeam(beamUp, x, y-1)
				d.traceEnergyBeam(beamDown, x, y+1)
				return
			}
		} else if t == horizSplitter {
			if dir == beamUp || dir == beamDown {
				//	fmt.Printf("Splitting beam in dir %v to go left and right at (%d,%d)\n", dir, x, y)
				d.traceEnergyBeam(beamLeft, x-1, y)
				d.traceEnergyBeam(beamRight, x+1, y)
				return
			}
		}

		//fmt.Printf("Moving in dir %v from (%d, %d) to ", dir, x, y)
		x, y = d.moveBeamInDirection(dir, x, y)
		//	fmt.Printf("(%d, %d)\n", x, y)
	}
}

func (d *Day16) countTilesVisited() int {
	sum := 0
	for y := 0; y < d.height; y++ {
		for x := 0; x < d.width; x++ {
			if d.visitedRows[y][x] > 0 {
				sum++
			}
		}
	}
	return sum
}

func (d *Day16) executePart1() {
	d.outputPlan()
	d.debugCounter = 0

	d.traceEnergyBeam(beamRight, 0, 0)
	count := d.countTilesVisited()
	d.outputEnergy()

	fmt.Printf("Tiles visited %d\n", count)
}

func (d *Day16) executePart2() {
	d.debugCounter = 0

	maxCount := 0
	var maxDirection beamDirection
	var maxX int
	var maxY int
	for x := 0; x < d.width; x++ {
		d.clearWorkings()
		d.traceEnergyBeam(beamDown, x, 0)
		count := d.countTilesVisited()
		if count > maxCount {
			maxCount = count
			maxX = x
			maxY = 0
			maxDirection = beamDown
		}
		d.clearWorkings()
		d.traceEnergyBeam(beamUp, x, d.height-1)
		count = d.countTilesVisited()
		if count > maxCount {
			maxCount = count
			maxX = x
			maxY = d.height - 1
			maxDirection = beamUp

		}
	}

	for y := 0; y < d.height; y++ {
		d.clearWorkings()
		d.traceEnergyBeam(beamRight, 0, y)
		count := d.countTilesVisited()
		if count > maxCount {
			maxX = 0
			maxY = y
			maxDirection = beamRight
			maxCount = count
		}
		d.clearWorkings()
		d.traceEnergyBeam(beamLeft, d.width-1, y)
		count = d.countTilesVisited()
		if count > maxCount {
			maxX = d.width - 1
			maxY = y
			maxDirection = beamLeft
			maxCount = count
		}
	}

	fmt.Printf("Tiles visited %d\nDirection %v from (%d,%d)\n", maxCount, maxDirection, maxX, maxY)
}
