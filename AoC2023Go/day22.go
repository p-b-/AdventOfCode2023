package main

// Part 1: 503
// Part 2: 98431

import (
	"fmt"
	"sort"
	"strconv"
	"strings"
)

type IntCoord3d struct {
	x int
	y int
	z int
}

func Create3dCoord(x, y, z int) IntCoord3d {
	return IntCoord3d{x, y, z}
}

func ParseCoord(coordAsString string) IntCoord3d {
	elements := strings.Split(coordAsString, ",")
	x, _ := strconv.Atoi(elements[0])
	y, _ := strconv.Atoi(elements[1])
	z, _ := strconv.Atoi(elements[2])

	return Create3dCoord(x, y, z)
}

type Day22Brick struct {
	brickId        int
	brickName      string
	supportedByIds []int
	supportingIds  []int
	lowerNearLeft  IntCoord3d
	upperFarRight  IntCoord3d
}

func createBrickFromCoords(id int, c1, c2 IntCoord3d) *Day22Brick {
	b := new(Day22Brick)
	b.brickId = id
	b.brickName = fmt.Sprintf("%c", 64+id)
	b.lowerNearLeft = c1
	b.upperFarRight = c2

	return b
}

func (b *Day22Brick) addSupportedById(brickIdSupportedBy int) {
	b.supportedByIds = append(b.supportedByIds, brickIdSupportedBy)
}

func (b *Day22Brick) addSupportingBrick(brickIdSupprting int) {
	b.supportingIds = append(b.supportingIds, brickIdSupprting)
}

func (b *Day22Brick) moveBlockDown(dropBy int) {
	b.lowerNearLeft.z = b.lowerNearLeft.z - dropBy
	b.upperFarRight.z = b.upperFarRight.z - dropBy
}

type Day22 struct {
	BaseDay

	cubes [][][]int

	bricks     []*Day22Brick
	bricksById map[int]*Day22Brick

	lowestNearLeft IntCoord3d
	upperFarRight  IntCoord3d

	nextBrickId int
	firstLine   bool
	lineNumber  int
}

func (d *Day22) readFile(part int) {
	d.firstLine = true
	d.nextBrickId = 1
	d.bricksById = make(map[int]*Day22Brick)

	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day22) parseLine(line string) {
	defer d.incLineNumber()

	lineLen := len(line)
	if lineLen == 0 {
		return
	}
	if d.firstLine {
		d.firstLine = false
	}

	coords := strings.Split(line, "~")
	coord1 := ParseCoord(coords[0])
	coord2 := ParseCoord(coords[1])
	brick := createBrickFromCoords(d.nextBrickId, coord1, coord2)
	d.nextBrickId++
	d.bricks = append(d.bricks, brick)
	d.bricksById[brick.brickId] = brick

	if brick.lowerNearLeft.x < d.lowestNearLeft.x {
		d.lowestNearLeft.x = brick.lowerNearLeft.x
	}
	if brick.lowerNearLeft.y < d.lowestNearLeft.y {
		d.lowestNearLeft.y = brick.lowerNearLeft.y
	}
	if brick.lowerNearLeft.z < d.lowestNearLeft.z {
		d.lowestNearLeft.z = brick.lowerNearLeft.z
	}

	if brick.upperFarRight.x > d.upperFarRight.x {
		d.upperFarRight.x = brick.upperFarRight.x
	}
	if brick.upperFarRight.y > d.upperFarRight.y {
		d.upperFarRight.y = brick.upperFarRight.y
	}
	if brick.upperFarRight.z > d.upperFarRight.z {
		d.upperFarRight.z = brick.upperFarRight.z
	}
}

func (d *Day22) incLineNumber() {
	d.lineNumber += 1
}

func (d *Day22) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day22) executePart1() {
	fmt.Println("\nExecute part 1")
	d.initialiseArena()
	d.sortBricks()
	d.placeBricks()
	d.makeBricksFall()

	for _, b := range d.bricks {
		d.determineBricksSupportingBrick(b)
	}
	for _, b := range d.bricks {
		d.determineBricksSupportedByBrick(b)
	}
	count := 0
	for _, b := range d.bricks {
		canBeDisintegrated := true
		for _, supportingBid := range b.supportingIds {
			supportedBrick := d.bricksById[supportingBid]
			if len(supportedBrick.supportedByIds) == 1 {
				canBeDisintegrated = false
				break
			}
		}
		if canBeDisintegrated {
			count++
			fmt.Printf("Brick %s can be disintegrated\n", b.brickName)
		}
	}
	fmt.Printf("%d can be disintegrated\n", count)
}

func (d *Day22) executePart2() {
	fmt.Println("\nExecute part 2")
	d.initialiseArena()
	d.sortBricks()
	d.placeBricks()
	d.makeBricksFall()

	for _, b := range d.bricks {
		d.determineBricksSupportingBrick(b)
	}
	for _, b := range d.bricks {
		d.determineBricksSupportedByBrick(b)
	}
	count := 0
	cannotBeDisintegratedCount := 0
	for _, b := range d.bricks {
		doesNotCauseChainReaction := true

		for _, supportingBid := range b.supportingIds {
			supportedBrick := d.bricksById[supportingBid]
			if len(supportedBrick.supportedByIds) == 1 {
				doesNotCauseChainReaction = false
				break
			}
		}
		if doesNotCauseChainReaction {
			cannotBeDisintegratedCount++
		}
		bricksFalling := make(map[int]bool)

		if !doesNotCauseChainReaction {
			d.bricksThatFallAfterDisintegratingBrick(b, bricksFalling)
			count += len(bricksFalling)
		}
	}

	fmt.Printf("Part 1 count %d\n", cannotBeDisintegratedCount)
	fmt.Printf("Chain reaction :%d\n", count)
}

func (d *Day22) bricksThatFallAfterDisintegratingBrick(disintegrateBrick *Day22Brick, bricksFalling map[int]bool) {
	if _, ok := bricksFalling[disintegrateBrick.brickId]; ok {
		return
	}
	for _, bid := range disintegrateBrick.supportingIds {
		if _, ok := bricksFalling[bid]; !ok {
			// Brick hasn't yet fallen
			brick := d.bricksById[bid]

			stillSupported := false
			for _, supportingBrickId := range brick.supportedByIds {
				if supportingBrickId != disintegrateBrick.brickId {
					if _, disintegrated := bricksFalling[supportingBrickId]; !disintegrated {
						stillSupported = true
					}
				}
			}
			if !stillSupported {
				d.bricksThatFallAfterDisintegratingBrick(brick, bricksFalling)
				bricksFalling[brick.brickId] = true
			}
		}
	}
}

func (d *Day22) sortBricks() {
	sort.SliceStable(d.bricks, func(i, j int) bool {
		return d.bricks[i].lowerNearLeft.z < d.bricks[j].lowerNearLeft.z
	})
}

func (d *Day22) initialiseArena() {
	fmt.Printf("Creating arena sized %d x %d x %d\n", d.upperFarRight.x, d.upperFarRight.y, d.upperFarRight.z)
	d.cubes = make([][][]int, d.upperFarRight.z+2)
	// Add extra layer to remove a boundary check when look at cubes above the top brick
	for z := 0; z <= d.upperFarRight.z+1; z++ {
		d.cubes[z] = make([][]int, d.upperFarRight.y+1)
		for y := 0; y <= d.upperFarRight.y; y++ {
			d.cubes[z][y] = make([]int, d.upperFarRight.x+1)
		}
	}
	fmt.Printf("%d x %d x %d\n", len(d.cubes), len(d.cubes[0]), len(d.cubes[0][0]))
}

func (d *Day22) placeBricks() {
	for _, b := range d.bricks {
		d.placeBrick((b))
	}
}

func (d *Day22) placeBrick(brick *Day22Brick) {
	for z := brick.lowerNearLeft.z; z <= brick.upperFarRight.z; z++ {
		for y := brick.lowerNearLeft.y; y <= brick.upperFarRight.y; y++ {
			for x := brick.lowerNearLeft.x; x <= brick.upperFarRight.x; x++ {

				d.cubes[z][y][x] = brick.brickId
			}
		}
	}
}

func (d *Day22) removeBrick(brick *Day22Brick) {
	for z := brick.lowerNearLeft.z; z <= brick.upperFarRight.z; z++ {
		for y := brick.lowerNearLeft.y; y <= brick.upperFarRight.y; y++ {
			for x := brick.lowerNearLeft.x; x <= brick.upperFarRight.x; x++ {
				d.cubes[z][y][x] = 0
			}
		}
	}
}

func (d *Day22) makeBricksFall() int {
	totalDropped := 0
	for _, b := range d.bricks {
		dropBy := d.brickCanFallBy(b)
		if dropBy != 0 {
			totalDropped++
			d.moveBlockDown(b, dropBy)
		}
	}
	return totalDropped
}

func (d *Day22) brickCanFallBy(b *Day22Brick) int {
	if b.lowerNearLeft.z == 1 {
		return 0
	}
	shortestDrop := d.upperFarRight.z + 1
	for x := b.lowerNearLeft.x; x <= b.upperFarRight.x; x++ {
		for y := b.lowerNearLeft.y; y <= b.upperFarRight.y; y++ {
			emptyBlocksBelow := d.emptyBlocksBelow(x, y, b.lowerNearLeft.z)

			if emptyBlocksBelow == 0 {
				return 0
			}
			if emptyBlocksBelow < shortestDrop {
				shortestDrop = emptyBlocksBelow
			}
		}
	}

	return shortestDrop
}

func (d *Day22) determineBricksSupportingBrick(b *Day22Brick) {
	lowerZ := b.lowerNearLeft.z - 1
	if lowerZ == 0 {
		return
	}
	brickIds := make(map[int]bool)

	for x := b.lowerNearLeft.x; x <= b.upperFarRight.x; x++ {
		for y := b.lowerNearLeft.y; y <= b.upperFarRight.y; y++ {
			idBelow := d.cubes[lowerZ][y][x]
			if idBelow != 0 {
				brickIds[idBelow] = true
			}
		}
	}
	for bid := range brickIds {
		b.addSupportedById(bid)
	}
}

func (d *Day22) determineBricksSupportedByBrick(b *Day22Brick) {
	upperZ := b.upperFarRight.z + 1

	brickIds := make(map[int]bool)

	for x := b.lowerNearLeft.x; x <= b.upperFarRight.x; x++ {
		for y := b.lowerNearLeft.y; y <= b.upperFarRight.y; y++ {
			idAbove := d.cubes[upperZ][y][x]
			if idAbove != 0 {
				brickIds[idAbove] = true
			}
		}
	}
	for bid := range brickIds {
		b.addSupportingBrick(bid)
	}
}

func (d *Day22) emptyBlocksBelow(x, y, z int) int {
	for searchZ := z - 1; searchZ > 0; searchZ-- {
		if d.cubes[searchZ][y][x] != 0 {
			return z - searchZ - 1
		}
	}
	return z - 1
}

func (d *Day22) moveBlockDown(b *Day22Brick, dropBy int) {
	d.removeBrick(b)
	b.moveBlockDown(dropBy)
	d.placeBrick(b)
}
