package main

import (
	"fmt"
	"strconv"
	"strings"
)

// Part 1 420739
// Part 2 130251901420382

type Day19Range struct {
	minInclusive int64
	maxInclusive int64
}

func (dr *Day19Range) sumOfElements() int64 {
	return dr.maxInclusive - dr.minInclusive + 1
}

func (dr *Day19Range) narrowClauseTrue(clause Day19WorkflowClause) Day19Range {
	var toReturn Day19Range
	toReturn.minInclusive = dr.minInclusive
	toReturn.maxInclusive = dr.maxInclusive
	if clause.compareGreaterNotLesser {
		if toReturn.minInclusive <= int64(clause.compareTo) {
			toReturn.minInclusive = int64(clause.compareTo + 1)
		}
	} else {
		if toReturn.maxInclusive >= int64(clause.compareTo) {
			toReturn.maxInclusive = int64(clause.compareTo - 1)
		}
	}
	return toReturn
}

func (dr *Day19Range) narrowClauseFalse(clause Day19WorkflowClause) Day19Range {
	var toReturn Day19Range
	toReturn.minInclusive = dr.minInclusive
	toReturn.maxInclusive = dr.maxInclusive
	if clause.compareGreaterNotLesser {
		if toReturn.maxInclusive > int64(clause.compareTo) {
			toReturn.maxInclusive = int64(clause.compareTo)
		}
	} else {
		if toReturn.minInclusive < int64(clause.compareTo) {
			toReturn.minInclusive = int64(clause.compareTo)
		}
	}
	return toReturn
}

type Day19MetalSubpart struct {
	name  string
	value int
}

type Day19MetalPart struct {
	subparts map[string]Day19MetalSubpart
}

func (mp *Day19MetalPart) SumParts() int {
	sum := 0
	for _, sp := range mp.subparts {
		sum += sp.value
	}
	return sum
}

type Day19ClauseResult struct {
	accept             bool
	reject             bool
	jumpToWorkflowName string
}

type Day19WorkflowClause struct {
	element                 string
	noComparison            bool
	compareGreaterNotLesser bool
	compareTo               int

	resultToExecute Day19ClauseResult
}

func (wfc *Day19WorkflowClause) ParseResult(clauseResultStr string) {
	if len(clauseResultStr) == 1 {
		if clauseResultStr == "A" {
			wfc.resultToExecute.accept = true
			return
		} else if clauseResultStr == "R" {
			wfc.resultToExecute.reject = true
			return
		}
	}

	wfc.resultToExecute.jumpToWorkflowName = clauseResultStr
}

type Day19Workflow struct {
	name string

	clauses []Day19WorkflowClause
}

type Day19 struct {
	BaseDay

	workflows     map[string]*Day19Workflow
	startWorkflow *Day19Workflow
	parts         []Day19MetalPart

	parsingWorkflows bool

	firstLine  bool
	lineNumber int
}

func (d *Day19) readFile(part int) {
	d.workflows = make(map[string]*Day19Workflow)

	d.parsingWorkflows = true
	d.firstLine = true
	fmt.Printf("Reading files %s\n", d.filepath)
	d.ReadFileAndParse(d.filepath, d.parseLine)
}

func (d *Day19) parseLine(line string) {
	if d.firstLine {
		d.firstLine = false
	}
	lineLen := len(line)
	if lineLen == 0 && d.parsingWorkflows {
		d.parsingWorkflows = false
	}
	if lineLen > 0 {
		if d.parsingWorkflows {
			d.parseWorkflow(line)
		} else {
			d.parseMetalPart(line)
		}
	}

	d.lineNumber = d.lineNumber + 1
}

func (d *Day19) parseWorkflow(line string) {
	endOfWorkflowName := strings.Index(line, "{")
	workflowName := line[0:endOfWorkflowName]

	workflow := new(Day19Workflow)
	workflow.name = workflowName

	workflowDetails := line[endOfWorkflowName+1 : len(line)-1]

	workflowClauses := strings.Split(workflowDetails, ",")

	//fmt.Printf("Workflow name: %s\n", workflowName)
	for _, wc := range workflowClauses {
		indexOfColon := strings.Index(wc, ":")
		var clause Day19WorkflowClause
		if indexOfColon != -1 {
			clause = d.parseComparison(wc[:indexOfColon])
			clause.ParseResult(wc[indexOfColon+1:])
		} else {
			clause.ParseResult(wc)
			clause.noComparison = true
		}
		workflow.clauses = append(workflow.clauses, clause)
		//fmt.Printf(" Clause: %v\n", clause)
	}

	d.workflows[workflowName] = workflow

	if workflow.name == "in" {
		d.startWorkflow = workflow
	}
}

func (d *Day19) parseComparison(comparisonStr string) Day19WorkflowClause {
	indexOfComparitor := strings.Index(comparisonStr, ">")
	comparitorIsGreaterThan := true
	if indexOfComparitor == -1 {
		indexOfComparitor = strings.Index(comparisonStr, "<")
		comparitorIsGreaterThan = false
	}

	var clause Day19WorkflowClause
	clause.noComparison = false
	clause.compareGreaterNotLesser = comparitorIsGreaterThan
	clause.element = comparisonStr[:indexOfComparitor]
	clause.compareTo, _ = strconv.Atoi(comparisonStr[indexOfComparitor+1:])
	return clause
}

func (d *Day19) parseMetalPart(line string) {
	var metalPart Day19MetalPart
	metalPart.subparts = make(map[string]Day19MetalSubpart)
	line = line[1 : len(line)-1]
	elements := strings.Split(line, ",")
	for _, e := range elements {
		elementParts := strings.Split(e, "=")
		subpart := new(Day19MetalSubpart)
		subpart.name = elementParts[0]
		subpart.value, _ = strconv.Atoi(elementParts[1])

		metalPart.subparts[subpart.name] = *subpart
	}
	d.parts = append(d.parts, metalPart)
}

func (d *Day19) execute(part int) {
	if part == 0 {
		d.executePart1()
	} else {
		d.executePart2()
	}
}

func (d *Day19) executePart1() {
	fmt.Println("Execute part 1")
	sum := 0
	// part := d.parts[2]
	// if d.executeWorkflowsAgainstPart(part) {
	// 	fmt.Printf("*** Part 2 is accepted, sum is %d\n", part.SumParts())

	// }
	for _, p := range d.parts {
		//fmt.Printf("Part %d\n", n)
		if d.executeWorkflowsAgainstPart(p) {
			//fmt.Printf("*** Part %d is accepted, sum is %d\n", n, p.SumParts())
			sum += p.SumParts()
		}
	}
	fmt.Printf("Sum of accepted parts is %d\n", sum)
}

func (d *Day19) executePart2() {
	fmt.Println("Execute part 2")
	xRange := Day19Range{1, 4000}
	mRange := Day19Range{1, 4000}
	aRange := Day19Range{1, 4000}
	sRange := Day19Range{1, 4000}

	var sum int64 = d.recurse(d.startWorkflow, xRange, mRange, aRange, sRange)
	fmt.Printf("Sum of accepted states is %d\n", sum)
}

func (d *Day19) executeWorkflowsAgainstPart(part Day19MetalPart) bool {
	workflow := d.startWorkflow
	//fmt.Printf(" Executing against workflow %s\n", workflow.name)
	for n := 0; n < len(workflow.clauses); n++ {
		clause := workflow.clauses[n]

		executeClause := false
		if clause.noComparison {
			//fmt.Printf(" No comparison, execute directly\n")
			// Execute clause directly
			executeClause = true
		} else {
			partValue := part.subparts[clause.element].value

			if clause.compareGreaterNotLesser {
				if partValue > clause.compareTo {
					//fmt.Printf(" %d > %d, execute\n", partValue, clause.compareTo)
					executeClause = true
				} /*else {
					fmt.Printf(" ! %d > %d, do not execute\n", partValue, clause.compareTo)
				}*/
			} else {
				if partValue < clause.compareTo {
					//fmt.Printf(" %d < %d, execute\n", partValue, clause.compareTo)
					executeClause = true
				} /* else {
					fmt.Printf(" ! %d < %d, do not execute\n", partValue, clause.compareTo)
				}*/
			}
		}
		if executeClause {
			//fmt.Printf("  Execute clause %d %v\n", n, clause.resultToExecute)
			if clause.resultToExecute.accept {
				//fmt.Printf("   Accept\n")
				return true
			} else if clause.resultToExecute.reject {
				//fmt.Printf("   Reject\n")
				return false
			}
			//fmt.Printf("   Change to workflow %s\n", clause.resultToExecute.jumpToWorkflowName)
			workflow = d.workflows[clause.resultToExecute.jumpToWorkflowName]
			n = -1
		} /*else {
			fmt.Printf("  Do not execute clause %d\n", n)
		}*/
	}
	return false
}

func (d *Day19) multiplyRanges(xRange, mRange, aRange, sRange Day19Range) int64 {
	var product int64 = xRange.sumOfElements()
	product *= mRange.sumOfElements()
	product *= aRange.sumOfElements()
	product *= sRange.sumOfElements()

	return product
}

func (d *Day19) recurse(workflow *Day19Workflow, xRange, mRange, aRange, sRange Day19Range) int64 {
	var sum int64 = 0
	for _, clause := range workflow.clauses {
		if clause.noComparison {
			if clause.resultToExecute.accept {
				sum += +d.multiplyRanges(xRange, mRange, aRange, sRange)
			} else if clause.resultToExecute.reject {
			} else {
				nextWorkflowName := clause.resultToExecute.jumpToWorkflowName
				nextWorkflow := d.workflows[nextWorkflowName]
				sum += d.recurse(nextWorkflow, xRange, mRange, aRange, sRange)
			}
		} else {
			var recurseXRange Day19Range = xRange
			var recurseMRange Day19Range = mRange
			var recurseARange Day19Range = aRange
			var recurseSRange Day19Range = sRange
			if clause.element == "x" {
				recurseXRange = xRange.narrowClauseTrue(clause)
				xRange = xRange.narrowClauseFalse(clause)
			} else if clause.element == "m" {
				recurseMRange = mRange.narrowClauseTrue(clause)
				mRange = mRange.narrowClauseFalse(clause)
			} else if clause.element == "a" {
				recurseARange = aRange.narrowClauseTrue(clause)
				aRange = aRange.narrowClauseFalse(clause)
			} else if clause.element == "s" {
				recurseSRange = sRange.narrowClauseTrue(clause)
				sRange = sRange.narrowClauseFalse(clause)
			}

			if clause.resultToExecute.accept {
				sum += +d.multiplyRanges(recurseXRange, recurseMRange, recurseARange, recurseSRange)
			} else if clause.resultToExecute.reject {
				continue
			} else {
				nextWorkflowName := clause.resultToExecute.jumpToWorkflowName
				nextWorkflow := d.workflows[nextWorkflowName]
				sum += d.recurse(nextWorkflow, recurseXRange, recurseMRange, recurseARange, recurseSRange)
			}
		}
	}
	return sum
}
