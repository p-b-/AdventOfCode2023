package main

// Part 1: 680278040
// Part 2: 243548140870057
import (
	"fmt"
	"strings"
)

type D20LogicUnitType int

const (
	untyped = iota
	broadcast
	flipflop
	conjunction
)

func (ut D20LogicUnitType) String() string {
	switch ut {
	case untyped:
		return "untyped"
	case broadcast:
		return "broadcast"
	case flipflop:
		return "flipflop"
	case conjunction:
		return "conjunction"
	}

	return "unknown"
}

type D20Pulse struct {
	ToUnitId     int
	ToInputIndex int
	PulseIsHigh  bool
}

func (d D20Pulse) String() string {
	if d.PulseIsHigh {
		return "High"
	} else {
		return "Low"
	}
}

type D20LogicUnitOutput struct {
	outputId              int
	connectedToInputIndex int
}

type D20LogicUnit struct {
	name        string
	logicType   D20LogicUnitType
	logicUnitId int
	outputStr   string

	inputs []bool

	logicUnitOn bool

	outputs []D20LogicUnitOutput
}

func (lu *D20LogicUnit) String() string {
	var toReturn string
	toReturn = fmt.Sprintf("LU %d, name %s, type %v, outputs: ", lu.logicUnitId, lu.name, lu.logicType)
	for n, outputId := range lu.outputs {
		if n > 0 {
			toReturn += fmt.Sprintf(",%d", outputId)
		} else {
			toReturn += fmt.Sprintf("%d", outputId)
		}
	}
	return toReturn
}

func (lu *D20LogicUnit) SetId(luId int) {
	lu.logicUnitId = luId
}

func (lu *D20LogicUnit) SetName(name string) {
	lu.name = name
}

func (lu *D20LogicUnit) SetLogicType(luType D20LogicUnitType) {
	lu.logicType = luType
	if luType == flipflop {
		lu.logicUnitOn = false
	}
}

func (lu *D20LogicUnit) SetOutputString(outputStr string) {
	lu.outputStr = outputStr
}

func (lu *D20LogicUnit) GetOutputString() string {
	return lu.outputStr
}

func (lu *D20LogicUnit) addInput() int {
	if lu == nil {
		panic("addInput passed a nil this pointer")
	}
	indexToReturn := len(lu.inputs)
	lu.inputs = append(lu.inputs, false)

	return indexToReturn
}

func (lu *D20LogicUnit) addOutput(outputId int, connectedInputIndex int) {
	lu.outputs = append(lu.outputs, D20LogicUnitOutput{outputId, connectedInputIndex})
}

func (lu *D20LogicUnit) ReceivePulse(inputIndex int, pulseIsHigh bool) []*D20Pulse {
	//fmt.Printf("Logic unit %s received pulse %t on input %d\n", lu.name, pulseIsHigh, inputIndex)
	lu.inputs[inputIndex] = pulseIsHigh

	if lu.logicType == broadcast {
		lu.logicUnitOn = pulseIsHigh
		return lu.pulsesForAllOutputs(pulseIsHigh)
	} else if lu.logicType == flipflop {
		if !pulseIsHigh {
			// Flip flops only do things when they receive a low pulse
			lu.logicUnitOn = !lu.logicUnitOn
			return lu.pulsesForAllOutputs(lu.logicUnitOn)
		}
	} else if lu.logicType == conjunction {
		allHigh := true
		for _, inputHigh := range lu.inputs {
			if !inputHigh {
				allHigh = false
			}
		}
		lu.logicUnitOn = !allHigh
		return lu.pulsesForAllOutputs(!allHigh)
	}

	return nil
}

func (lu *D20LogicUnit) pulsesForAllOutputs(pulseIsHigh bool) []*D20Pulse {
	pulseOutputs := make([]*D20Pulse, 0)
	for _, output := range lu.outputs {
		pulseOutputs = append(pulseOutputs, &D20Pulse{output.outputId, output.connectedToInputIndex, pulseIsHigh})
	}
	return pulseOutputs
}

type Day20 struct {
	BaseDay

	logicUnitsByName map[string]*D20LogicUnit
	logicUnitsById   map[int]*D20LogicUnit
	broadcastId      int
	pulseQueue       []*D20Pulse

	lastStateChange  map[int]int
	firstStateChange map[int]int
	stateTime        int

	nextLUId   int
	firstLine  bool
	lineNumber int
}

func (d *Day20) readFile(part int) {
	d.logicUnitsByName = make(map[string]*D20LogicUnit)
	d.logicUnitsById = make(map[int]*D20LogicUnit)

	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day20) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	lineLen := len(line)
	if lineLen > 0 {
		d.parseLogicUnit(line)
	}

	d.lineNumber = d.lineNumber + 1
}

func (d *Day20) parseLogicUnit(line string) {
	indexOfArrow := strings.Index(line, "->")
	luTypeString := line[0 : indexOfArrow-1]

	var luType D20LogicUnitType = untyped
	logicUnitName := "unnamed"
	if luTypeString == "broadcaster" {
		luType = broadcast
		logicUnitName = "broadcaster"
		d.broadcastId = d.nextLUId
	} else if luTypeString[0] == '%' {
		luType = flipflop
		logicUnitName = luTypeString[1:]
	} else if luTypeString[0] == '&' {
		luType = conjunction
		logicUnitName = luTypeString[1:]
	}

	logicUnit := d.createLogicUnit(luType, logicUnitName)
	logicUnit.SetOutputString(line[indexOfArrow+3:])
}

func (d *Day20) createLogicUnit(logicType D20LogicUnitType, name string) *D20LogicUnit {
	logicUnit := new(D20LogicUnit)
	logicUnit.SetLogicType(logicType)
	logicUnit.SetId(d.nextLUId)
	logicUnit.SetName(name)
	d.logicUnitsById[logicUnit.logicUnitId] = logicUnit
	d.logicUnitsByName[name] = logicUnit
	d.nextLUId++

	return logicUnit
}

func (d *Day20) connectLogicUnits() {
	for id := 0; id < d.nextLUId; id++ {
		logicUnit := d.logicUnitsById[id]
		if logicUnit == nil {
			panic(fmt.Sprintf("unit %d does not seem to exist\n", id))
		}
		outputs := strings.Split(logicUnit.GetOutputString(), ",")
		for _, o := range outputs {
			o = strings.Trim(o, " ")
			var connectToLogicUnit *D20LogicUnit
			if len(o) > 0 {
				if unit, ok := d.logicUnitsByName[o]; ok {
					connectToLogicUnit = unit
				} else {
					// Connected to a logic unit that hasn't been named specifically in input list
					connectToLogicUnit = d.createLogicUnit(untyped, o)
				}
				inputIndex := connectToLogicUnit.addInput()

				logicUnit.addOutput(connectToLogicUnit.logicUnitId, inputIndex)
			}
		}
	}

	broadcastUnit := d.logicUnitsById[d.broadcastId]
	broadcastUnit.addInput()
}

func (d *Day20) outputLogicUnits() {
	for luIndex := 0; luIndex < d.nextLUId; luIndex++ {
		lu := d.logicUnitsById[luIndex]
		fmt.Printf("Logic unit %v\n", lu)
	}
}

func (d *Day20) queuePulseAsObject(pulse *D20Pulse) {
	d.pulseQueue = append(d.pulseQueue, pulse)
}

func (d *Day20) pulsesAreQueued() bool {
	return len(d.pulseQueue) > 0
}

func (d *Day20) dequeuePulse() (*D20Pulse, bool) {
	if len(d.pulseQueue) == 0 {
		return nil, false
	} else {
		pulse := d.pulseQueue[0]
		d.pulseQueue = d.pulseQueue[1:]
		return pulse, true
	}
}

func (d *Day20) execute(part int) {
	d.connectLogicUnits()
	d.outputLogicUnits()
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day20) executePart1() {
	fmt.Println("\nExecute part 1")

	var totalHighCount int64
	var totalLowCount int64
	for pressCount := 0; pressCount < 1000; pressCount++ {
		d.PressButton()
		lowSubCount, highSubCount := d.ProcessPulses()
		totalHighCount += highSubCount
		totalLowCount += lowSubCount
	}
	fmt.Printf("Pulse count is low:%d high:%d\n", totalLowCount, totalHighCount)

	product := totalLowCount * totalHighCount
	fmt.Printf("Total is %d\n", product)
}

func (d *Day20) executePart2() {
	fmt.Println("\nExecute part 2")
	ofInterest := []string{"hf", "mk", "pm", "pk"}
	d.InitialiseState()

	d.stateTime = 1
	allFound := true
	for {
		d.PressButton()
		d.ProcessPulsesPart2(ofInterest)
		allFound = true
		for _, s := range ofInterest {
			logicUnit := d.logicUnitsByName[s]
			if d.lastStateChange[logicUnit.logicUnitId] == -1 {
				allFound = false
				break
			}
		}
		if allFound {
			break
		}
		d.stateTime++
	}
	var lcmThese []int
	if allFound {
		for _, s := range ofInterest {
			logicUnit := d.logicUnitsByName[s]
			lastStateChangeTime := d.lastStateChange[logicUnit.logicUnitId]
			firstChange := d.firstStateChange[logicUnit.logicUnitId]
			diff := lastStateChangeTime - firstChange
			lcmThese = append(lcmThese, diff)
			fmt.Printf("%s period is %d (%d-%d)\n", logicUnit.name, diff, lastStateChangeTime, firstChange)
		}
	}
	// LCM those numbers.  Multiply together for now
	var product int64 = 1
	for _, v := range lcmThese {
		product = product * int64(v)
	}
	fmt.Printf("Result is %d\n", product)
}

func (d *Day20) PressButton() {
	pulse := &D20Pulse{d.broadcastId, 0, false}

	d.queuePulseAsObject(pulse)
}

func (d *Day20) ProcessPulses() (int64, int64) {
	var lowPulseTotal int64 = 0
	var highPulseTotal int64 = 0
	for d.pulsesAreQueued() {
		pulse, ok := d.dequeuePulse()
		if !ok {
			panic("Could not deque pulse")
		}
		if pulse.PulseIsHigh {
			highPulseTotal++
		} else {
			lowPulseTotal++
		}
		logicUnit := d.logicUnitsById[pulse.ToUnitId]
		morePulses := logicUnit.ReceivePulse(pulse.ToInputIndex, pulse.PulseIsHigh)
		for _, pulse := range morePulses {
			d.queuePulseAsObject(pulse)
		}
	}
	return lowPulseTotal, highPulseTotal
}

func (d *Day20) ProcessPulsesPart2(ofInterest []string) bool {

	for d.pulsesAreQueued() {
		pulse, ok := d.dequeuePulse()
		if !ok {
			panic("Could not deque pulse")
		}

		logicUnit := d.logicUnitsById[pulse.ToUnitId]
		currentState := logicUnit.logicUnitOn
		morePulses := logicUnit.ReceivePulse(pulse.ToInputIndex, pulse.PulseIsHigh)
		if logicUnit.logicUnitOn && currentState != logicUnit.logicUnitOn && d.contains(ofInterest, logicUnit.name) {

			firstChangeStateTime := d.firstStateChange[logicUnit.logicUnitId]
			if firstChangeStateTime == -1 {
				d.firstStateChange[logicUnit.logicUnitId] = d.stateTime
				fmt.Printf("%d First state change for %s\n", d.stateTime, logicUnit.name)
			} else {
				firstChangeTime := d.firstStateChange[logicUnit.logicUnitId]
				d.lastStateChange[logicUnit.logicUnitId] = d.stateTime
				diff := d.stateTime - firstChangeTime
				fmt.Printf("%d Changed state for %s, first change %d, diff %d\n", d.stateTime, logicUnit.name, firstChangeTime, diff)
			}
		}
		for _, pulse := range morePulses {
			d.queuePulseAsObject(pulse)
		}
	}
	return false
}

func (d *Day20) InitialiseState() {
	d.lastStateChange = make(map[int]int)
	d.firstStateChange = make(map[int]int)
	d.stateTime = 1

	for logicId := 0; logicId < d.nextLUId; logicId++ {
		d.lastStateChange[logicId] = -1
		d.firstStateChange[logicId] = -1
	}
}

func (d *Day20) contains(strs []string, c string) bool {
	for _, s := range strs {
		if s == c {
			return true
		}
	}
	return false
}
