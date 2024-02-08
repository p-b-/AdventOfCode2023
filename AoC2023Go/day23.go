package main

import (
	"fmt"
	"strings"
)

// Part 1: 2238
// Part 2: 6392 too low, 7098 too high
// 6398?

// type IntCoord2d struct {
// 	x int
// 	y int
// }

type Day23Direction int

const (
	D23Left Day23Direction = iota
	D23Right
	D23Up
	D23Down
)

func (dd Day23Direction) String() string {
	switch dd {
	case D23Left:
		return "left"
	case D23Right:
		return "right"
	case D23Up:
		return "up"
	case D23Down:
		return "down"
	}
	return "Unspecified direction"
}

type D23GraphNodeIterator struct {
	node         *D23GraphNode
	dist         int
	visitedFlags uint64
}

type D23GraphNodeConnection struct {
	distance int
	node     *D23GraphNode
}

type D23GraphNode struct {
	identifier  int
	x           int
	y           int
	flag        uint64
	connections []*D23GraphNodeConnection
}

func (gn *D23GraphNode) AddConnection(otherNode *D23GraphNode, dist int) {
	newConnection := &D23GraphNodeConnection{dist, otherNode}
	gn.connections = append(gn.connections, newConnection)
}

func (gn *D23GraphNode) connectedTo(otherId int) bool {
	for _, otherNode := range gn.connections {
		if otherNode.node.identifier == otherId {
			return true
		}
	}
	return false
}

type Day23Node struct {
	prevX int
	prevY int

	x          int
	y          int
	pathLength int
}

func (dn *Day23Node) moveInDirection(dir Day23Direction, d *Day23) *Day23Node {
	var nextX = dn.x
	var nextY = dn.y
	switch dir {
	case D23Left:
		if dn.x == 0 || dn.prevX == dn.x-1 {
			return nil
		} else if d.rows[dn.y][dn.x-1] == '>' {
			return nil
		}
		nextX--
	case D23Right:
		if dn.x == d.width-1 || dn.prevX == dn.x+1 {
			return nil
		}
		nextX++
	case D23Up:
		if dn.y == 0 || dn.prevY == dn.y-1 {
			return nil
		} else if d.rows[dn.y-1][dn.x] == 'v' {
			return nil
		}
		nextY--
	case D23Down:
		if dn.y == d.height-1 || dn.prevY == dn.y+1 {
			return nil
		}
		nextY++
	}

	return &Day23Node{dn.x, dn.y, nextX, nextY, dn.pathLength + 1}
}

type Day23 struct {
	BaseDay

	rows   []string
	width  int
	height int
	startX int
	startY int
	endX   int
	endY   int

	startNodeId int

	// Exit is the position next to the end.  Reaching this can be classed as finishing
	exitX int
	exitY int

	longestPath int

	nodes      []*Day23Node
	pathLength [][]int

	graphNodes        map[int]*D23GraphNode
	graphQueue        []*D23GraphNodeIterator
	nextGraphNodeFlag uint64

	firstLine  bool
	lineNumber int
}

func (d *Day23) readFile(part int) {
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day23) parseLine(line string) {
	defer d.incLineNumber()

	lineLen := len(line)
	if lineLen == 0 {
		return
	}
	if d.firstLine {
		d.firstLine = false
		d.width = len(line)
		for x := 0; x < d.width; x++ {
			if line[x] == '.' {
				d.startX = x
				d.startY = 0
				break
			}
		}
	}

	d.rows = append(d.rows, line)
	d.height = len(d.rows)
}

func (d *Day23) incLineNumber() {
	d.lineNumber += 1
}

func (d *Day23) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day23) executePart1() {
	fmt.Printf("\nExecute part 1")
	d.initialise(0)
	fmt.Printf("\nStart at (%d,%d)\n", d.startX, d.startY)
	fmt.Printf("End at (%d,%d)\n", d.endX, d.endY)
	fmt.Printf("Size %dx%d\n", d.width, d.height)

	d.queueNode(d.startX, d.startY, d.startX, d.startY, 0)

	d.processNodes()

	fmt.Printf("Longest path is %d\n", d.longestPath)
}

func (d *Day23) executePart2() {
	fmt.Println("\nExecute part 2")

	d.initialise(1)

	fmt.Printf("\nStart at (%d,%d)\n", d.startX, d.startY)
	fmt.Printf("End at (%d,%d)\n", d.endX, d.endY)
	fmt.Printf("Size %dx%d\n", d.width, d.height)

	d.createGraph()
	d.displayGraph()

	d.queueIteratorWithNodeId(d.startNodeId)
	d.processGraphQueue()

	fmt.Printf("Longest path is %d\n", d.longestPath)
}

func (d *Day23) initialise(part int) {
	// Find endpoint
	for x := 0; x < d.width; x++ {
		if d.rows[d.height-1][x] == '.' {
			d.endX = x
			d.endY = d.height - 1
			break
		}
	}
	d.pathLength = make([][]int, d.height)
	for y := 0; y < d.height; y++ {
		d.pathLength[y] = make([]int, d.width)
	}

	if part == 1 {
		d.startX, d.endX = d.endX, d.startX
		d.startY, d.endY = d.endY, d.startY
		// Get rid of slopes
		for y := 0; y < d.height; y++ {
			d.rows[y] = strings.ReplaceAll(d.rows[y], ">", ".")
			d.rows[y] = strings.ReplaceAll(d.rows[y], "v", ".")
		}
		d.nextGraphNodeFlag = 1
	} else {
		d.exitX = d.endX
		d.exitY = d.endY - 1
	}

	d.graphNodes = make(map[int]*D23GraphNode)
}

func (d *Day23) queueNode(prevX, prevY int, x, y int, currentLength int) {
	node := &Day23Node{prevX, prevY, x, y, currentLength}
	d.nodes = append(d.nodes, node)
}

func (d *Day23) queueNodeObject(node *Day23Node) {
	d.nodes = append(d.nodes, node)
}

func (d *Day23) dequeueNode() *Day23Node {
	if len(d.nodes) == 0 {
		return nil
	}
	toReturn := d.nodes[0]
	d.nodes = d.nodes[1:]
	return toReturn
}

func (d *Day23) processNodes() {
	dirs := []Day23Direction{D23Left, D23Right, D23Up, D23Down}
	for len(d.nodes) > 0 {
		node := d.dequeueNode()
		for _, dir := range dirs {
			nextNode := node.moveInDirection(dir, d)
			if nextNode == nil {
				continue
			}
			if nextNode.x == d.endX && nextNode.y == d.endY {
				d.foundRoute(nextNode, 0)
			} else {
				element := d.rows[nextNode.y][nextNode.x]
				if element == '.' || element == '>' || element == 'v' {
					d.queueNodeObject(nextNode)
				}
			}
			/*(else if d.pathLength[nextNode.y][nextNode.x] <= nextNode.pathLength {
				d.pathLength[nextNode.y][nextNode.x] = nextNode.pathLength
				element := d.rows[nextNode.y][nextNode.x]
				if element == '.' || element == '>' || element == 'v' {
					d.queueNodeObject(nextNode)
				}
			}*/
		}
	}
}

func (d *Day23) foundRoute(n *Day23Node, addToPathLength int) {
	if n.pathLength > d.longestPath {
		d.longestPath = n.pathLength + addToPathLength
		fmt.Printf("Found longer path %d, nodes count %d\n", n.pathLength+addToPathLength, len(d.nodes))
		// } else {
		// 	fmt.Printf("Found path %d\n", n.pathLength)
		// }
	}
}

func (d *Day23) createGraph() {
	x := d.startX
	y := d.startY
	prevNodeId, _ := d.createGraphNode(x, y)
	d.startNodeId = prevNodeId
	d.followPathToNextNode(x, y, x, y, prevNodeId, false)
}

func (d *Day23) displayGraph() {
	fmt.Printf("\nGraph:\n")
	for k, v := range d.graphNodes {
		fmt.Printf("Node: %d at (%d,%d) connects to:\n", k, v.x, v.y)
		for _, connection := range v.connections {
			fmt.Printf(" (%d,%d) distance %d\n", connection.node.x, connection.node.y, connection.distance)
		}
	}
	fmt.Printf("\n%d nodes\n\n", len(d.graphNodes))
}

func (d *Day23) followPathToNextNode(x, y, prevX, prevY int, prevNodeId int, includeStartingSquare bool) {
	directions := d.getDirectionsAtPosition(x, y, prevX, prevY)
	distance := 0
	if includeStartingSquare {
		distance++
	}
	for len(directions) == 1 {
		prevX = x
		prevY = y
		x, y = d.moveInDirection(x, y, directions[0])
		directions = d.getDirectionsAtPosition(x, y, prevX, prevY)
		distance++
	}
	nodeId, createdThisIteration := d.createGraphNode(x, y)
	if d.alreadyConnected(prevNodeId, nodeId) {
		return
	}
	d.createConnectionBetween(prevNodeId, nodeId, distance)
	if !createdThisIteration {
		return
	}

	if len(directions) > 1 {
		prevX, prevY = x, y
		for _, dir := range directions {
			//fmt.Printf("Moving from (%d,%d) in direction %v\n", prevX, prevY, dir)
			x, y = d.moveInDirection(prevX, prevY, dir)
			d.followPathToNextNode(x, y, prevX, prevY, nodeId, true)
		}
	}
}

func (d *Day23) moveInDirection(x, y int, dir Day23Direction) (int, int) {
	switch dir {
	case D23Left:
		return x - 1, y
	case D23Right:
		return x + 1, y
	case D23Up:
		return x, y - 1
	case D23Down:
		return x, y + 1
	}
	panic("Unknown direction")
}

func (d *Day23) getDirectionsAtPosition(x, y, prevX, prevY int) []Day23Direction {
	directions := make([]Day23Direction, 0, 4)
	if x != 0 && prevX != x-1 && d.rows[y][x-1] == '.' {
		directions = append(directions, D23Left)
	}
	if x != d.width-1 && prevX != x+1 && d.rows[y][x+1] == '.' {
		directions = append(directions, D23Right)
	}
	if y != 0 && prevY != y-1 && d.rows[y-1][x] == '.' {
		directions = append(directions, D23Up)
	}
	if y != d.height-1 && prevY != y+1 && d.rows[y+1][x] == '.' {
		directions = append(directions, D23Down)
	}
	return directions
}

func (d *Day23) createGraphNode(x, y int) (int, bool) {
	created := false
	gnId := x + y*d.width
	if _, ok := d.graphNodes[gnId]; !ok {
		newNode := &D23GraphNode{gnId, x, y, d.nextGraphNodeFlag, nil}
		d.nextGraphNodeFlag <<= 1
		d.graphNodes[gnId] = newNode
		created = true
		//fmt.Printf("Create node at (%d,%d) [%d]\n", x, y, gnId)
	}
	return gnId, created
}

func (d *Day23) createConnectionBetween(gnId1, gnId2, dist int) {
	node1 := d.graphNodes[gnId1]
	node2 := d.graphNodes[gnId2]
	//fmt.Printf("Connecting (%d,%d) -> (%d,%d) distance %d\n", node1.x, node1.y, node2.x, node2.y, dist)
	node1.AddConnection(node2, dist)
	node2.AddConnection(node1, dist)
}

func (d *Day23) alreadyConnected(gnId1, gnId2 int) bool {
	node1 := d.graphNodes[gnId1]
	return node1.connectedTo(gnId2)
}

func (d *Day23) queueIteratorWithNodeId(nodeId int) {
	node := d.graphNodes[nodeId]
	iter := &D23GraphNodeIterator{node, 0, 0}
	d.graphQueue = append(d.graphQueue, iter)
}

func (d *Day23) queueIterator(iter *D23GraphNodeIterator) {
	d.graphQueue = append(d.graphQueue, iter)
}

func (d *Day23) dequeueIterator() *D23GraphNodeIterator {
	iter := d.graphQueue[0]
	d.graphQueue = d.graphQueue[1:]
	return iter
}

func (d *Day23) processGraphQueue() {
	for len(d.graphQueue) > 0 {
		iter := d.dequeueIterator()

		if iter.node.x == d.endX && iter.node.y == d.endY {
			d.foundRouteToEndNode(iter, 0)
			continue
		} else {
			for _, connection := range iter.node.connections {
				if (iter.visitedFlags & connection.node.flag) == 0 {
					// not visited yet, queue a visit!
					newIter := &D23GraphNodeIterator{connection.node, iter.dist + connection.distance, iter.visitedFlags | connection.node.flag}
					d.queueIterator(newIter)
				}
			}
		}
	}
}

func (d *Day23) foundRouteToEndNode(iter *D23GraphNodeIterator, addToPathLength int) {
	if iter.dist > d.longestPath {
		d.longestPath = iter.dist + addToPathLength
		fmt.Printf("Found longer path %d, nodes count %d\n", d.longestPath, len(d.graphQueue))
		// } else {
		// 	fmt.Printf("Found path %d\n", n.pathLength)
		// }
	}
}
