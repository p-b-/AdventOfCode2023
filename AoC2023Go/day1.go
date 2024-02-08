package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"strconv"
)

type Day1 struct {
	filepath string
	lines    []string
}

func NewDay1(filepath string) *Day1 {
	fmt.Println("Constructing")
	d := new(Day1)
	d.filepath = filepath
	d.readFile()

	return d
}

func (d *Day1) readFile() {
	fmt.Println("Reading")
	f, err := os.Open(d.filepath)
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println("Opened")
	// remember to close the file at the end of the program
	defer f.Close()

	// read the file line by line using scanner
	scanner := bufio.NewScanner(f)

	for scanner.Scan() {
		d.lines = append(d.lines, scanner.Text())
	}

	if err := scanner.Err(); err != nil {
		log.Fatal(err)
	}
}

func (d *Day1) Execute() {
	fmt.Println("Starting")
	var sum int
	sum = 0
	for _, line := range d.lines {
		sum += processLineForElvesOnlyDigits(line)
	}
	fmt.Printf("Sum is %v\n", sum)
	fmt.Println("Finished")
}

func processLineForElvesOnlyDigits(line string) int {
	foundDigit, digit1 := firstIndexOfDigit(line)

	if !foundDigit {
		//fmt.Printf("%v : %d\n", line, 0)
		return 0
	}

	_, digit2 := lastIndexOfDigit(line)

	//fmt.Printf("Line %v\n", line)
	//fmt.Printf(" Digit 1 at %d, digit 2 at %d\n", digit1, digit2)

	var d1 = line[digit1 : digit1+1]
	var d2 = line[digit2 : digit2+1]
	//fmt.Printf(" Digts are %v and %v\n", d1, d2)

	var digits = d1 + d2

	var i, _ = strconv.Atoi(digits)
	//fmt.Printf("%v : %d\n", line, i)
	return i
}

func firstIndexOfDigit(line string) (bool, int) {
	for i, r := range line {
		if r >= '0' && r <= '9' {
			return true, i
		}
	}
	return false, 0
}

func lastIndexOfDigit(line string) (bool, int) {
	for i := range line {
		r := line[len(line)-i-1]
		if r >= '0' && r <= '9' {
			return true, len(line) - i - 1
		}
	}
	return false, 0

}
