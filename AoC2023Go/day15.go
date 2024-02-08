package main

import (
	"fmt"
	"strconv"
	"strings"
)

// Part 1: 513172
// Part 2: 237806

type Day15Lense struct {
	label       string
	focalLength int
}

type Day15Box struct {
	boxId  int
	lenses []Day15Lense
}

func (db *Day15Box) LenseCount() int {
	return len(db.lenses)
}

func (db *Day15Box) OutputLenses() {
	for _, lense := range db.lenses {
		fmt.Printf(" [%s %d]", lense.label, lense.focalLength)
	}
}

func (db *Day15Box) CalculatePower() int {
	power := 0
	for index, lense := range db.lenses {
		power += (db.boxId + 1) * (index + 1) * lense.focalLength
	}
	return power
}

func (db *Day15Box) findLenseIndex(lenseLabel string) int {
	for n, l := range db.lenses {
		if l.label == lenseLabel {
			return n
		}
	}
	return -1
}

func (db *Day15Box) removeLense(lenseLabel string) {
	lenseIndex := db.findLenseIndex(lenseLabel)
	if lenseIndex != -1 {
		db.lenses = append(db.lenses[:lenseIndex], db.lenses[lenseIndex+1:]...)
	}
}

func (db *Day15Box) addLense(lenseLabel string, focalLength int) {
	lenseIndex := db.findLenseIndex(lenseLabel)
	if lenseIndex != -1 {
		db.lenses[lenseIndex].focalLength = focalLength
	} else {
		newLense := Day15Lense{lenseLabel, focalLength}
		db.lenses = append(db.lenses, newLense)
	}
}

type Day15 struct {
	BaseDay

	boxes map[int]*Day15Box

	line       string
	firstLine  bool
	lineNumber int
}

func (d *Day15) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day15) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	if len(line) > 0 {
		d.line = line
	}

	//fmt.Printf("%s\n", line)
	d.lineNumber = d.lineNumber + 1
}

func (d *Day15) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day15) executePart1() {
	toHash := strings.Split(d.line, ",")
	var sum int64
	fmt.Printf("%d elementts\n", len(toHash))
	for _, element := range toHash {
		var hash = d.hash(element)
		sum += int64(hash)
		fmt.Printf("Hash of %s is %d\n", element, hash)
	}

	fmt.Printf("\n\nSum is %d\n", sum)
}

func (d *Day15) executePart2() {
	d.boxes = make(map[int]*Day15Box)

	toHash := strings.Split(d.line, ",")
	var sum int64
	fmt.Printf("%d elementts\n", len(toHash))
	for _, element := range toHash {
		if strings.Contains(element, "-") {
			d.removeLenseFromBox(element[:len(element)-1])
		} else if strings.Contains(element, "=") {
			elementSplitByEquals := strings.Split(element, "=")
			focalLength, _ := strconv.Atoi(elementSplitByEquals[1])
			d.addLenseToBox(elementSplitByEquals[0], focalLength)
		}
		//	d.outputLenses()
		var hash = d.hash(element)
		sum += int64(hash)
	}
	fmt.Println()
	d.outputLenses()

	power := d.calculatePower()

	fmt.Printf("\n\n Power is %d\n", power)
}

func (d *Day15) HashWithOutput(toHash string) int {
	var hashValue int
	for _, c := range toHash {
		fmt.Printf(" Adding %d\n", byte(c))
		hashValue += int(c)

		fmt.Printf("  %d\n", hashValue)
		hashValue *= 17
		fmt.Printf("  *17 = %d\n", hashValue)
		hashValue &= 0xff
		fmt.Printf("  %%256  = %d\n", hashValue)
	}

	return hashValue
}

func (d *Day15) hash(toHash string) int {
	var hashValue int
	for _, c := range toHash {
		hashValue += int(c)

		hashValue *= 17
		hashValue &= 0xff
	}

	return hashValue
}

func (d *Day15) removeLenseFromBox(label string) {
	boxId := d.hash(label)
	fmt.Printf("Remove %s from box %d\n", label, boxId)
	box := d.getBox(boxId)
	box.removeLense(label)
	if box.LenseCount() == 0 {
		d.removeBox(boxId)
	}
}

func (d *Day15) addLenseToBox(label string, focalLength int) {
	boxId := d.hash(label)
	fmt.Printf("Add %s to box %d with focal length %d\n", label, boxId, focalLength)
	box := d.getBox(boxId)
	box.addLense(label, focalLength)
}

func (d *Day15) getBox(boxId int) *Day15Box {

	box, ok := d.boxes[boxId]
	if !ok {
		box = new(Day15Box)
		box.boxId = boxId
		d.boxes[boxId] = box
	}
	return box
}

func (d *Day15) removeBox(boxId int) {
	_, ok := d.boxes[boxId]
	if ok {
		delete(d.boxes, boxId)
	}
}

func (d *Day15) outputLenses() {
	for boxNumber := 0; boxNumber < 256; boxNumber++ {
		box, boxExists := d.boxes[boxNumber]
		if boxExists {
			fmt.Printf("Box %d:", box.boxId)
			box.OutputLenses()
			fmt.Println()
		}
	}
}

func (d *Day15) calculatePower() int {
	power := 0
	for boxNumber := 0; boxNumber < 256; boxNumber++ {
		box, boxExists := d.boxes[boxNumber]
		if boxExists {
			power += box.CalculatePower()
		}
	}
	return power
}
