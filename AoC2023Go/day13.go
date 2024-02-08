package main

import "fmt"

// Part 1: 34202 correct
// Part 2: 34230 correct

type Day13 struct {
	BaseDay
	firstLine               bool
	lineNumber              int
	currentMirrorBeingParse *Day13Mirror

	mirrors []Day13Mirror
}

type Day13Mirror struct {
	MirrorIndex int
	lines       [][]bool
	columns     [][]bool
	width       int

	rowLeftJustifiedPos  map[int]bool
	rowRightJustifiedPos map[int]bool
}

func (dm *Day13Mirror) MirrorSize() (int, int) {
	return len(dm.lines[0]), len(dm.lines)
}

func (dm *Day13Mirror) FlipElement(x int, y int) {
	fmt.Printf("M%d flipping (%d,%d) from %t:%t", dm.MirrorIndex, x, y,
		dm.lines[y][x], dm.columns[x][y])
	current := dm.lines[y][x]
	if current {
		dm.lines[y][x] = false
		dm.columns[x][y] = false
	} else {
		dm.lines[y][x] = true
		dm.columns[x][y] = true
	}
	fmt.Printf(" to %t:%t\n", dm.lines[y][x], dm.columns[x][y])
}

func (dm *Day13Mirror) Calculate(doNotReturn int) int {
	dm.assessAllReversePositions(dm.lines[0])
	for rowIndex := 1; rowIndex < len(dm.lines); rowIndex++ {
		dm.assessReversePositions(dm.lines[rowIndex])
	}
	outputHorizRight := dm.getReflectionToRight(dm.width)
	outputHorizLeft := dm.getReflectionToLeft(dm.width)
	fmt.Printf("M%d hl %d, hr %d dnr %d\n", dm.MirrorIndex, outputHorizLeft, outputHorizRight, doNotReturn)
	if doNotReturn == outputHorizRight {
		outputHorizRight = -1
	}
	if doNotReturn == outputHorizLeft {
		outputHorizLeft = -1
	}

	var outputHoriz = outputHorizLeft
	if outputHorizRight > outputHorizLeft {
		outputHoriz = outputHorizRight
	}

	dm.assessAllReversePositions(dm.columns[0])
	for columnIndex := 1; columnIndex < len(dm.columns); columnIndex++ {
		dm.assessReversePositions(dm.columns[columnIndex])
	}
	outputVertBottom := dm.getReflectionToRight(len(dm.columns[0]))
	outputVertTop := dm.getReflectionToLeft(len(dm.columns[0]))
	fmt.Printf("M%d vt %d, vb %d dnr %d\n", dm.MirrorIndex, outputVertTop, outputVertBottom, doNotReturn)

	if doNotReturn == outputVertBottom*100 {
		outputVertBottom = -1
	}
	if doNotReturn == outputVertTop*100 {
		outputVertTop = -1
	}
	var outputVert = outputVertTop
	if outputVertBottom > outputVertTop {
		outputVert = outputVertBottom
	}

	if outputHoriz > outputVert {
		fmt.Printf("M%d return horiz sum %d (not vert sum %d)\n", dm.MirrorIndex, outputHoriz, outputVert*100)
		if outputHoriz == doNotReturn && outputVert != 0 {
			fmt.Printf("  actually returning vert sum %d\n", outputVert*100)
			return outputVert * 100
		} else {
			return outputHoriz
		}
	} else {
		fmt.Printf("M%d return vertical sum %d (not horiz sum %d)\n", dm.MirrorIndex, outputVert*100, outputHoriz)
		if outputVert*100 == doNotReturn && outputHoriz != 0 {
			fmt.Printf("  actually returning horiz sum %d\n", outputHoriz)
			return outputHoriz
		} else {
			return outputVert * 100
		}
	}
}

func (dm *Day13Mirror) ConcatenateFlags(flags []bool) string {
	var toReturn string
	for _, f := range flags {
		if f {
			toReturn += "*"
		} else {
			toReturn += "."
		}
	}
	return toReturn
}

func (dm *Day13Mirror) AddLine(line string) {
	if len(dm.lines) == 0 {
		defaultHeight := 100
		dm.width = len(line)
		dm.lines = make([][]bool, 0, defaultHeight)
		dm.initialiseColumnData()
	}
	dm.addRow(line)

	for n, r := range line {
		if r == '#' {
			dm.columns[n] = append(dm.columns[n], true)
		} else {
			dm.columns[n] = append(dm.columns[n], false)
		}
	}
}

func (dm *Day13Mirror) addRow(row string) {
	rowFlags := make([]bool, dm.width)
	for n, r := range row {
		flag := r == '#'
		rowFlags[n] = flag
	}
	dm.lines = append(dm.lines, rowFlags)

	if len(dm.lines) == 1 {
		dm.assessAllReversePositions(dm.lines[0])
	} else {
		dm.assessReversePositions(rowFlags)
	}
}

func (dm *Day13Mirror) assessAllReversePositions(lineToAssess []bool) {
	lineLen := len(lineToAssess)
	dm.rowRightJustifiedPos = make(map[int]bool)
	dm.rowLeftJustifiedPos = make(map[int]bool)

	firstOutput := true
	for pos := 0; pos < lineLen; pos++ {
		var flag bool
		if pos == lineLen-1 {
			dm.rowRightJustifiedPos[pos] = false
		} else {
			flag = dm.canReverseLineFromIndexJustifiedRight(lineToAssess, pos)
			dm.rowRightJustifiedPos[pos] = flag
		}
		if flag {
			if firstOutput {
				//fmt.Printf("M%d Line %s reversible at justified right %d", dm.MirrorIndex, dm.concatenateFlags(lineToAssess), pos)
				firstOutput = false
			} /*else {
				fmt.Printf(", %d", pos)
			}*/
		}
	}
	firstLeftOutput := true
	for pos := lineLen - 1; pos >= 0; pos-- {
		// if pos == 0 {
		// 	dm.rowLeftJustifiedPos[pos] = false
		// } else {
		var flag bool = dm.canReverseLineFromIndexJustifiedLeft(lineToAssess, pos)
		dm.rowLeftJustifiedPos[pos] = flag
		//}
		if flag {
			if firstOutput {
				//fmt.Printf("M%d Line %s reversible at justified left %d", dm.MirrorIndex, dm.concatenateFlags(lineToAssess), pos)
				firstOutput = false
				firstLeftOutput = false
			} else if firstLeftOutput {
				//				fmt.Printf(", justified left %d", pos)
				firstLeftOutput = false
			} /*else {
				fmt.Printf(", %d", pos)
			}*/
		}
	}
}

func (dm *Day13Mirror) assessReversePositions(line []bool) {
	lineLen := len(line)
	firstOutput := true
	for pos := 0; pos < lineLen; pos++ {
		if dm.rowRightJustifiedPos[pos] {
			flag := dm.canReverseLineFromIndexJustifiedRight(line, pos)
			if flag {
				if firstOutput {
					//fmt.Printf("M%d Line %s reversible at justified right %d", dm.MirrorIndex, dm.concatenateFlags(line), pos)
					firstOutput = false
				} /*else {
					fmt.Printf(", %d", pos)
				}*/
			} else {
				dm.rowRightJustifiedPos[pos] = false
			}
		}
	}
	firstLeftOutput := true
	for pos := lineLen - 1; pos >= 0; pos-- {
		if dm.rowLeftJustifiedPos[pos] {
			flag := dm.canReverseLineFromIndexJustifiedLeft(line, pos)
			if flag {
				if firstOutput {
					//fmt.Printf("M%d Line %s reversible at justified left %d", dm.MirrorIndex, dm.concatenateFlags(line), pos)
					firstOutput = false
					firstLeftOutput = false
				} else if firstLeftOutput {
					//	fmt.Printf(", justified left %d", pos)
					firstLeftOutput = false
				} /*else {
					fmt.Printf(", %d", pos)
				}*/
			} else {
				dm.rowLeftJustifiedPos[pos] = false
			}
		}
	}
}

func (dm *Day13Mirror) getReflectionToRight(width int) int {
	maxToRight := 0
	for pos := 0; pos < width; pos++ {
		if dm.rowRightJustifiedPos[pos] {
			rowsToRight := (width-pos)/2 + pos
			if rowsToRight > maxToRight {
				maxToRight = rowsToRight
			}
		}
	}
	return maxToRight
}

func (dm *Day13Mirror) getReflectionToLeft(width int) int {
	maxToLeft := 0
	for pos := 0; pos < width; pos++ {
		if dm.rowLeftJustifiedPos[pos] {
			rowsToLeft := (pos + 1) / 2
			if rowsToLeft > maxToLeft {
				maxToLeft = rowsToLeft
			}
		}
	}
	return maxToLeft
}

func (dm *Day13Mirror) OutputReversePositions() {
	firstOutput := true
	for pos := 0; pos < dm.width; pos++ {
		if dm.rowRightJustifiedPos[pos] {
			rowsToLeft := (dm.width-pos)/2 + pos
			if firstOutput {
				fmt.Printf("Mirror %d reversible at justified right %d", dm.MirrorIndex, pos)
				fmt.Printf("(%d)", rowsToLeft)
				firstOutput = false
			} else {
				fmt.Printf(", %d", pos)
				fmt.Printf("(%d)", rowsToLeft)
			}
		}
	}
	firstLeftOutput := true
	for pos := dm.width - 1; pos >= 0; pos-- {
		if dm.rowLeftJustifiedPos[pos] && (pos%2) != 0 {
			rowsToLeft := (pos + 1) / 2
			if firstOutput {
				fmt.Printf("Mirror %d reversible at justified left %d", dm.MirrorIndex, pos)
				fmt.Printf("(%d)", rowsToLeft)
				firstOutput = false
				firstLeftOutput = false
			} else if firstLeftOutput {
				fmt.Printf(", justified left %d", pos)
				fmt.Printf("(%d)", rowsToLeft)
				firstLeftOutput = false
			} else {
				fmt.Printf(", %d", pos)
				fmt.Printf("(%d)", rowsToLeft)
			}
		}
	}
	if !firstOutput {
		fmt.Println()
	}
}

func (dm *Day13Mirror) canReverseLineFromIndexJustifiedRight(line []bool, index int) bool {
	lineLen := len(line)
	if (lineLen-index)%2 > 0 {
		return false
	}
	for n := index; n < lineLen; n++ {
		flag := line[n]
		opposingFlag := line[lineLen-1-(n-index)]
		if flag != opposingFlag {
			return false
		}
	}
	return true
}

func (dm *Day13Mirror) canReverseLineFromIndexJustifiedLeft(line []bool, index int) bool {
	if (index % 2) == 0 {
		return false
	}
	for n := index; n >= 0; n-- {
		flag := line[n]
		opposingFlag := line[(index - n)]
		if flag != opposingFlag {
			return false
		}
	}
	return true
}

func (dm *Day13Mirror) initialiseColumnData() {
	dm.columns = make([][]bool, dm.width)
}

func (dm *Day13Mirror) ReverseColumn(columnData []bool) []bool {
	height := len(columnData)
	toReturn := make([]bool, height)
	for n, f := range columnData {
		toReturn[height-n-1] = f
	}
	return toReturn
}

func (d *Day13) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day13) execute(part int) {
	var sum int64 = 0
	if part == 0 {
		for _, m := range d.mirrors {
			partSum := m.Calculate(-1)
			sum += int64(partSum)
		}
	} else {
		for _, m := range d.mirrors {
			partSum := m.Calculate(-1)
			newPartSum := d.findAlternativeAnswerForMirror(m, partSum)

			if newPartSum == partSum {
				fmt.Printf("M%d  PROBLEM.  Sum was %d, still %d\n", m.MirrorIndex, partSum, newPartSum)
			} else {
				fmt.Printf("M%d  sum was %d, now %d\n", m.MirrorIndex, partSum, newPartSum)
			}
			sum += int64(newPartSum)
		}

	}

	fmt.Printf("sum is %v\n", sum)
}

func (d *Day13) findAlternativeAnswerForMirror(m Day13Mirror, currentSum int) int {
	width, height := m.MirrorSize()
	for y := 0; y < height; y++ {
		for x := 0; x < width; x++ {
			m.FlipElement(x, y)
			newSum := m.Calculate(currentSum)
			if newSum != currentSum && newSum > 0 {
				fmt.Printf("M%d new sum is %d, flipped (%d,%d)\n", m.MirrorIndex, newSum, x, y)
				return newSum
			}
			m.FlipElement(x, y)
		}
	}
	return currentSum
}

func (d *Day13) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	if len(line) == 0 {
		if d.currentMirrorBeingParse != nil {
			d.mirrors = append(d.mirrors, *d.currentMirrorBeingParse)
			d.currentMirrorBeingParse = nil
		}
		// If no mirror being parsed, must be a spurious line break in input file
		return
	}
	if d.currentMirrorBeingParse == nil {
		//		fmt.Printf("New mirror!\n")
		d.currentMirrorBeingParse = new(Day13Mirror)
		d.currentMirrorBeingParse.MirrorIndex = len(d.mirrors)
	}
	d.currentMirrorBeingParse.AddLine(line)

	//	fmt.Printf("%s\n", line)
	d.lineNumber = d.lineNumber + 1
}
