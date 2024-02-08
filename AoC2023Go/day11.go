package main

import (
	"fmt"
	"strings"
)

// Part 1: 9274989
// Part 2: 357134560737

type day11Galaxy struct {
	X int64
	Y int64
}

type Day11 struct {
	BaseDay
	//	emptyLines    []int
	lineNumber    int64
	galaxies      []day11Galaxy
	firstLine     bool
	columnIsBlank []bool
	expansion     int64
}

func absInt64(v int64) int64 {
	if v < 0 {
		return -v
	}
	return v
}

func (d *Day11) readFile(part int) {
	if part == 0 {
		d.expansion = 1
	} else {
		d.expansion = 999999
	}
	fmt.Printf("Reading file %v\n", d.filepath)
	d.firstLine = true
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day11) execute(part int) {

	d.expandColumns()
	d.outputGalaxy()
	var sumOfDistances int64
	sumOfDistances = 0
	for index := 0; index < len(d.galaxies)-1; index++ {
		distancesForGalaxy := d.sumDistancesFromGalaxy(index)
		sumOfDistances += distancesForGalaxy
	}
	fmt.Printf("sum is %v\n", sumOfDistances)
}

func (d *Day11) sumDistancesFromGalaxy(galaxyIndex int) int64 {
	var distance int64
	if galaxyIndex == len(d.galaxies)-1 {
		return 0
	}
	gX := d.galaxies[galaxyIndex].X
	gY := d.galaxies[galaxyIndex].Y
	for _, g := range d.galaxies[galaxyIndex+1:] {
		subDistance := absInt64((int64)(g.X - gX))
		subDistance += absInt64((int64)(g.Y - gY))
		// Extra +1 here are because the puzzle is 1-based, not 0-based
		//fmt.Printf("Distance between galaxy %d and %d is %v\n", galaxyIndex+1, n+galaxyIndex+1+1, subDistance)
		distance += subDistance
	}
	return distance
}

func (d *Day11) outputGalaxy() {
	for _, g := range d.galaxies {
		fmt.Printf("Galaxy at (%d,%d)\n", g.X, g.Y)
	}
}

func (d *Day11) parseLine(line string) {
	if d.firstLine {
		d.initBlankColumnArray(len(line))
		d.firstLine = false
	}
	fmt.Printf("%v Line %v\n", d.lineNumber, line)
	if !strings.Contains(line, "#") {
		d.lineNumber += d.expansion
	} else {
		d.findGalaxiesInLine(line)
	}
	d.lineNumber = d.lineNumber + 1
}

func (d *Day11) expandColumns() {
	var addExpansion int64 = 0
	for c, columnIsBlank := range d.columnIsBlank {
		if columnIsBlank {
			for i, g := range d.galaxies {
				if g.X-addExpansion > (int64)(c) {
					g.X += d.expansion
					d.galaxies[i] = g
				}
			}
			addExpansion += d.expansion
		}
	}
}

func (d *Day11) initBlankColumnArray(columns int) {
	d.columnIsBlank = make([]bool, columns)
	for i := 0; i < columns; i++ {
		d.columnIsBlank[i] = true
	}
}

func (d *Day11) findGalaxiesInLine(line string) {
	indexIntoLine := 0
	index := strings.Index(line[indexIntoLine:], "#")
	for index != -1 {
		galaxy := day11Galaxy{(int64)(indexIntoLine + index), (int64)(d.lineNumber)}
		d.columnIsBlank[indexIntoLine+index] = false
		d.galaxies = append(d.galaxies, galaxy)
		indexIntoLine += index + 1
		index = strings.Index(line[indexIntoLine:], "#")
	}
}
