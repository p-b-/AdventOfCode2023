package main

// Part 1 106186
// Part 2 106389 not right (too low)
// Part 2 106390?

import (
	"fmt"
)

type elementType int

const (
	empty elementType = iota
	cubeRock
	roundRock
)

type Day14Rock struct {
	Id       int
	X        int
	Y        int
	RockType elementType
}

type Day14 struct {
	BaseDay
	firstLine  bool
	lineNumber int

	width   int
	depth   int
	lines   []string
	columns [][]int

	rocks  map[int]Day14Rock
	nextId int

	loopStart       int64
	weightsToRecord int
	recordedWeights []int64
	loopEnd         int64
	loopSize        int64
}

func (d *Day14) readFile(part int) {
	d.rocks = make(map[int]Day14Rock)
	d.nextId = 1

	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day14) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	if len(d.columns) == 0 {
		d.width = len(line)
		d.initialiseColumms(d.width)
	}
	d.addRowToColumns(len(d.columns[0]), line)
	d.depth = len(d.columns[0])

	d.lines = append(d.lines, line)
	fmt.Printf("%s\n", line)
	d.lineNumber = d.lineNumber + 1
}

func (d *Day14) initialiseColumms(width int) {
	d.columns = make([][]int, width)
}

func (d *Day14) weigh() int64 {
	var weight int64
	for z := 0; z < d.depth; z++ {
		for x := 0; x < d.width; x++ {
			if d.columns[x][z] > 0 {
				r := d.rocks[d.columns[x][z]]
				if r.RockType == roundRock {
					weight += int64(d.depth - z)
				}
			}
		}
	}
	return weight
}

func (d *Day14) outputFormation() {
	for z := 0; z < d.depth; z++ {
		for x := 0; x < d.width; x++ {
			if x == 0 {
				fmt.Printf("%4d ", z)
			}
			id := d.columns[x][z]
			if id == 0 {
				fmt.Print(".")
			} else {
				r := d.rocks[id]
				if r.RockType == roundRock {
					fmt.Printf("O")
				} else {
					fmt.Printf("#")
				}
			}
			fmt.Printf(" ")
		}
		fmt.Println()
	}
}

func (d *Day14) rollCycle(output bool) {
	if output {
		fmt.Printf("\n\nRolling cycle:\n")
	}
	d.rollRocksNorth()
	if output {
		fmt.Printf("Rolled north:\n")
		d.outputFormation()
	}

	d.rollRocksWest()
	if output {
		fmt.Printf("Rolled west:\n")
		d.outputFormation()
	}

	d.rollRocksSouth()
	if output {
		fmt.Printf("Rolled south:\n")
		d.outputFormation()
	}

	d.rollRocksEast()
	if output {
		fmt.Printf("Rolled east:\n")
		d.outputFormation()
	}

}

func (d *Day14) rollRocksNorth() {
	for x := 0; x < d.width; x++ {
		d.rollRockInColumnAlongZ(x, true)
	}
}

func (d *Day14) rollRocksSouth() {
	for x := 0; x < d.width; x++ {
		d.rollRockInColumnAlongZ(x, false)
	}
}

func (d *Day14) rollRocksEast() {
	for z := 0; z < d.depth; z++ {
		d.rollRockInRowAlongX(z, false)
	}
}

func (d *Day14) rollRocksWest() {
	for z := 0; z < d.depth; z++ {
		d.rollRockInRowAlongX(z, true)
	}
}

func (d *Day14) rollRockInColumnAlongZ(x int, rollNorthNotSound bool) {
	rollToZ := -1
	zStart := 0
	zInc := 1
	zEnd := d.depth
	zLimiter := d.depth - 1
	if !rollNorthNotSound {
		zStart = d.depth - 1
		zInc = -1
		zEnd = -1
		zLimiter = 0
	}
	for z := zStart; z != zEnd; z += zInc {
		id := d.columns[x][z]
		if id == 0 {
			if rollToZ == -1 {
				rollToZ = z
			}
		} else {
			if r, ok := d.rocks[id]; !ok || r.RockType == cubeRock {
				rollToZ = -1
			} else if r.RockType == roundRock {
				if rollToZ != -1 {
					d.columns[x][z] = 0
					d.columns[x][rollToZ] = id
					if z != zLimiter && d.columns[x][rollToZ+zInc] == 0 {
						rollToZ = rollToZ + zInc
					} else {
						rollToZ = -1
					}
				}
			}
		}
	}
}

func (d *Day14) rollRockInRowAlongX(z int, rollWestNotEast bool) {
	rollToX := -1
	xStart := 0
	xInc := 1
	xEnd := d.width
	xLimiter := d.width - 1
	if !rollWestNotEast {
		xStart = d.width - 1
		xInc = -1
		xEnd = -1
		xLimiter = 0
	}
	for x := xStart; x != xEnd; x += xInc {
		id := d.columns[x][z]
		if id == 0 {
			if rollToX == -1 {
				rollToX = x
			}
		} else {
			if r, ok := d.rocks[id]; !ok || r.RockType == cubeRock {
				rollToX = -1
			} else if r.RockType == roundRock {
				if rollToX != -1 {
					d.columns[x][z] = 0
					d.columns[rollToX][z] = id
					if x != xLimiter && d.columns[rollToX+xInc][z] == 0 {
						rollToX = rollToX + xInc
					} else {
						rollToX = -1
					}
				}
			}
		}
	}
}

func (d *Day14) addRowToColumns(z int, row string) {
	for x, r := range row {
		t := empty
		if r == 'O' {
			t = roundRock
		} else if r == '#' {
			t = cubeRock
		}
		var rock Day14Rock
		if t == roundRock || t == cubeRock {
			rock = Day14Rock{d.nextId, x, z, t}
			d.nextId++
			if t == roundRock {
				d.rocks[rock.Id] = rock
			}
			d.columns[x] = append(d.columns[x], rock.Id)
		} else {
			d.columns[x] = append(d.columns[x], 0)
		}

	}
}

func (d *Day14) execute(part int) {
	d.outputFormation()
	if part == 0 {
		d.rollRocksNorth()
		fmt.Printf("\nRolled\n\n")
		d.outputFormation()
		weight := d.weigh()
		fmt.Printf("Weight is %d\n", weight)
	} else if part == 1 {
		d.weightsToRecord = 1000
		d.recordedWeights = make([]int64, 0, d.weightsToRecord)
		d.loopEnd = -1
		d.loopSize = -1

		d.Part2()
	}
}

func (d *Day14) Part2() {
	var c int64
	var maxC int64 = 1000000000
	d.loopStart = 300000
	maxC = 100000000 + d.loopStart
	lookForLoop := false
	for c = 1; c <= maxC; c++ {
		d.rollCycle(false)
		var weight int64 = d.weigh()
		if c < d.loopStart && (c%50000) == 0 {
			fmt.Printf("Calculated weight[%d] = %d  (unrecorded) \n", c, weight)
		}

		if c == d.loopStart {
			fmt.Printf("Start looking for loop at %d\n", d.loopStart)
			fmt.Printf(" First weight is %d\n", weight)
			lookForLoop = true
			d.recordedWeights = append(d.recordedWeights, weight)
			d.loopEnd = d.loopStart
			d.loopSize = d.loopEnd - d.loopStart + 1
			fmt.Printf("Weight[%d]=%d\n", d.loopStart, weight)
		} else if lookForLoop {
			d.recordedWeights = append(d.recordedWeights, weight)

			offsetFromLoopStart := c - d.loopStart
			modulusSinceLoopStart := offsetFromLoopStart % d.loopSize
			fmt.Printf("Weight[%d]=%d   Offset into loop %d mod %d =  %d\n", c, weight, offsetFromLoopStart, d.loopSize, modulusSinceLoopStart)
			if d.recordedWeights[modulusSinceLoopStart] != weight {
				fmt.Printf(" Extending loop as %d != %d\n", d.recordedWeights[modulusSinceLoopStart], weight)
				d.loopEnd = c
				d.loopSize = c - d.loopStart + 1
			}
		}
		if c > d.loopStart+(int64)(d.weightsToRecord) {
			fmt.Printf("Finished, as c=%d  >  %d + %d\n", c, d.loopStart, d.weightsToRecord)
			break
		}
	}

	fmt.Printf("Loop size is %d\nLoop start is %d\nLoop end is %d\n", d.loopSize, d.loopStart, d.loopEnd)
	for n := d.loopStart; n <= d.loopEnd; n++ {
		fmt.Printf("weights[%d] = %d\n", n, d.recordedWeights[n-d.loopStart])
	}

	// c=0 is rolled once, not c=1
	// So count up to billion-1
	var distanceToBillion int64 = 1000000000 - d.loopStart
	modLoopSize := distanceToBillion % d.loopSize
	fmt.Printf("Distances to billion %d\n", distanceToBillion)
	fmt.Printf("Mod loop size %d\n", modLoopSize)
	fmt.Printf("Value at billion is %d\n", d.recordedWeights[modLoopSize])
}
